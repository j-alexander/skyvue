namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type MailService(db : DataStore, logon : ILogon) =
            
    let readInbox (token:string) (mailId:string) =
        let user = logon.Verify token
        let matches = query {
            for i in db.Inbox do
            join m in db.Mail on (i.MailId = m.MailId)
            where (i.UserId = user.UserId &&
                   i.MailId = mailId)
            select (i, m)
        }
        if Seq.length matches <> 1 then
            raise (NotFound "This mail could not be found.")
        Seq.head matches

    let readOutbox (token:string) (mailId:string) =
        let user = logon.Verify token
        let matches = query {
            for o in db.Outbox do
            join m in db.Mail on (o.MailId = m.MailId)
            where (o.UserId = user.UserId &&
                   o.MailId = mailId)
            select (o, m)
        }
        if Seq.length matches <> 1 then
            raise (NotFound "This mail could not be found.")
        Seq.head matches

    let listInbox (token:string) (size:int) (page:int) (flaggedOnly:bool) (unreadOnly:bool) =
        let user = logon.Verify token
        let matches = query {
            for i in db.Inbox do
            join m in db.Mail on (i.MailId = m.MailId)
            where (i.UserId = user.UserId &&
                  (if unreadOnly then i.IsRead = DataStore.no else true) &&
                  (if flaggedOnly then i.IsFlagged = DataStore.yes else true))
            sortByDescending m.DateSent
            skip (size * page)
            take (size)
            select (i, m)
        }
        Seq.toList matches

    let listOutbox (token:string) (size:int) (page:int) (flaggedOnly:bool) =
        let user = logon.Verify token
        let matches = query {
            for o in db.Outbox do
            join m in db.Mail on (o.MailId = m.MailId)
            where (o.UserId = user.UserId &&
                  (if flaggedOnly then o.IsFlagged = DataStore.yes else true))
            sortByDescending m.DateSent
            skip (size * page)
            take (size)
            select (o, m)
        }
        Seq.toList matches

    interface IMail with
        
        member x.CountUnread (token:string) =
            let user = logon.Verify token
            let mail = x :> IMail
            mail.CountUnreadForUser user.UserId

        member x.CountUnreadForUser (userId:string) =
            let matches = query {
                for m in db.Inbox do
                where (m.UserId = userId &&
                       m.IsRead = DataStore.no)
                count
            }
            matches

        member x.DeleteIn (token:string) (mailId:string) =
            let inbox, mail = readInbox token mailId
            db.Inbox.DeleteOnSubmit(inbox)
            db.DataContext.SubmitChanges()
            mail

        member x.DeleteOut (token:string) (mailId:string) =
            let outbox, mail = readOutbox token mailId
            db.Outbox.DeleteOnSubmit(outbox)
            db.DataContext.SubmitChanges()
            mail

        member x.FlagIn (token:string) (mailId:string) (flagged:bool) =
            let inbox, mail = readInbox token mailId
            inbox.IsFlagged <- if flagged then DataStore.yes else DataStore.no
            db.DataContext.SubmitChanges()
            mail

        member x.FlagOut (token:string) (mailId:string) (flagged:bool) =
            let outbox, mail = readOutbox token mailId
            outbox.IsFlagged <- if flagged then DataStore.yes else DataStore.no
            db.DataContext.SubmitChanges()
            mail

        member x.ListFlaggedIn (token:string) (size:int) (page:int) =
            listInbox token size page true false |> List.map snd

        member x.ListFlaggedOut (token:string) (size:int) (page:int) =
            listOutbox token size page true |> List.map snd

        member x.ListIn (token:string) (size:int) (page:int) =
            listInbox token size page false false |> List.map snd

        member x.ListOut (token:string) (size:int) (page:int) =
            listOutbox token size page false |> List.map snd

        member x.ListUnread (token:string) (size:int) (page:int) =
            listInbox token size page false true |> List.map snd

        member x.ReadIn (token:string) (mailId:string) (read:bool) =
            let inbox, mail = readInbox token mailId
            inbox.IsRead <- if read then DataStore.yes else DataStore.no
            db.DataContext.SubmitChanges()
            mail

        member x.Send (token:string) (subject:string) (contents:string) (userId:seq<string>) =
            let user = logon.Verify token

            // create the underlying mail item
            let mail = new Mail()
            mail.MailId <- DataStore.id()
            mail.SenderId <- user.UserId
            mail.DateSent <- DateTime.Now
            mail.Subject <- subject
            mail.Contents <- contents
            db.Mail.InsertOnSubmit(mail)

            for recipientId in userId do
                
                // verify the recipient exists
                let exists = query {
                    for u in db.User do
                    where (u.UserId = recipientId)
                    count
                }
                if exists <> 1 then
                    raise (NotFound "A desired recipient could not be found.")

                // create the recipient record
                let recipient = new Recipient()
                recipient.UserId <- recipientId
                recipient.MailId <- mail.MailId
                db.Recipient.InsertOnSubmit(recipient)

                // create the inbox item
                let inbox = new Inbox()
                inbox.InboxId <- DataStore.id()
                inbox.UserId <- recipientId
                inbox.MailId <- mail.MailId
                inbox.IsFlagged <- DataStore.no
                inbox.IsRead <- DataStore.no
                db.Inbox.InsertOnSubmit(inbox)

            // create the outbox item
            let outbox = new Outbox()
            outbox.OutboxId <- DataStore.id()
            outbox.MailId <- mail.MailId
            outbox.UserId <- user.UserId
            outbox.IsFlagged <- DataStore.no
            db.Outbox.InsertOnSubmit(outbox)

            // persist the changes
            db.DataContext.SubmitChanges()
            mail

        member x.ViewIn (token:string) (mailId:string) =
            let inbox, mail = readInbox token mailId
            mail

        member x.ViewOut (token:string) (mailId:string) =
            let outbox, mail = readOutbox token mailId
            mail