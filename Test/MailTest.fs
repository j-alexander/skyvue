namespace SkyVue.Test

open System
open System.Linq
open NUnit.Framework

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface
open SkyVue.Service

type UserToken = { token : string; user : User }

[<TestFixture>]
type MailTest() =

    let passworda = "a123"
    let passwordb = "b321"
    let passwordc = "c456"
    let subject = "subject"
    let contents = "contents"

    let sendFromAToBC () =
        let logon = new LogonService(Database.connect()) :> ILogon
        let mail = new MailService(Database.connect(), logon) :> IMail
        
        let a,b,c =
            let a = logon.Logon "a" passworda
            let b = logon.Logon "b" passwordb
            let c = logon.Logon "c" passwordc
            { token = a ; user = logon.Verify a },
            { token = b ; user = logon.Verify b },
            { token = c ; user = logon.Verify c }

        let letter = mail.Send a.token subject contents [b.user.UserId ; c.user.UserId]
        a, b, c, letter, mail

    [<SetUp>]
    member x.SetUp() =
        Database.create()
        let join = new JoinService(Database.connect()) :> IJoin
        let a = join.Join "a" passworda "J1" "a@test.com"
        let b = join.Join "b" passwordb "J2" "b@test.com"
        let c = join.Join "c" passwordc "K1" "c@test.com"
        ()

    [<TearDown>]
    member x.TearDown() =
        Database.delete()

    [<Test>]
    member x.TestSendMail() =
        let a, b, c, letter, mail = sendFromAToBC()
        
        Assert.AreEqual(a.user.UserId, letter.SenderId)
        Assert.AreEqual(subject, letter.Subject)
        Assert.AreEqual(contents, letter.Contents)
        ()

    [<Test>]
    member x.TestReadAndUnread() =
        let a, b, c, letter, mail = sendFromAToBC()

        let unread forA forB forC =
            Assert.AreEqual(forA, mail.CountUnread a.token)
            Assert.AreEqual(forB, mail.CountUnread b.token)
            Assert.AreEqual(forC, mail.CountUnread c.token)
            Assert.AreEqual(forA, mail.CountUnreadForUser a.user.UserId)
            Assert.AreEqual(forB, mail.CountUnreadForUser b.user.UserId)
            Assert.AreEqual(forC, mail.CountUnreadForUser c.user.UserId)
            Assert.AreEqual(forA, mail.ListUnread a.token 10 0 |> List.length)
            Assert.AreEqual(forB, mail.ListUnread b.token 10 0 |> List.length)
            Assert.AreEqual(forC, mail.ListUnread c.token 10 0 |> List.length)

        unread 0 1 1
        mail.ReadIn b.token letter.MailId true |> ignore
        unread 0 0 1
        mail.ReadIn c.token letter.MailId true |> ignore
        unread 0 0 0
        mail.ReadIn b.token letter.MailId false |> ignore
        unread 0 1 0
        mail.ReadIn c.token letter.MailId false |> ignore
        unread 0 1 1
        try
            let result = mail.ReadIn a.token letter.MailId true
            Assert.Fail()
        with
        | exn -> ()

    [<Test>]
    member x.TestViewInAndOut() =
        let a, b, c, letter, mail = sendFromAToBC()

        let verify (x : Mail) =
            Assert.AreEqual(letter.MailId, x.MailId)
            Assert.AreEqual(letter.SenderId, x.SenderId)
            Assert.AreEqual(letter.DateSent, x.DateSent)
            Assert.AreEqual(letter.Subject, x.Subject)
            Assert.AreEqual(letter.Contents, x.Contents)
        
        verify (mail.ViewOut a.token letter.MailId)
        verify (mail.ViewIn b.token letter.MailId)
        verify (mail.ViewIn c.token letter.MailId)

    [<Test>]
    member x.TestFlagInAndOut() =
        let a, b, c, letter, mail = sendFromAToBC()

        let flagged forA forB forC =
            Assert.AreEqual(forA, mail.ListFlaggedOut a.token 10 0 |> List.length)
            Assert.AreEqual(forB, mail.ListFlaggedIn b.token 10 0 |> List.length)
            Assert.AreEqual(forC, mail.ListFlaggedIn c.token 10 0 |> List.length)

        flagged 0 0 0
        mail.FlagOut a.token letter.MailId true |> ignore
        flagged 1 0 0
        mail.FlagIn b.token letter.MailId true |> ignore
        flagged 1 1 0
        mail.FlagIn c.token letter.MailId true |> ignore
        flagged 1 1 1
        mail.FlagIn b.token letter.MailId false |> ignore
        flagged 1 0 1
        mail.FlagOut a.token letter.MailId false |> ignore
        flagged 0 0 1
        mail.FlagIn c.token letter.MailId false |> ignore
        flagged 0 0 0

    [<Test>]
    member x.TestDelete() =
        let a, b, c, letter, mail = sendFromAToBC()
        
        let verify (x : Mail) =
            Assert.AreEqual(letter.MailId, x.MailId)
            Assert.AreEqual(letter.SenderId, x.SenderId)
            Assert.AreEqual(letter.DateSent, x.DateSent)
            Assert.AreEqual(letter.Subject, x.Subject)
            Assert.AreEqual(letter.Contents, x.Contents)
        
        let count forA forB forC =
            Assert.AreEqual(forA, mail.ListOut a.token 10 0 |> List.length)
            Assert.AreEqual(forB, mail.ListIn b.token 10 0 |> List.length)
            Assert.AreEqual(forC, mail.ListIn c.token 10 0 |> List.length)

        count 1 1 1
        mail.DeleteOut a.token letter.MailId |> verify
        count 0 1 1
        mail.DeleteIn c.token letter.MailId |> verify
        count 0 1 0
        mail.DeleteIn b.token letter.MailId |> verify
        count 0 0 0
        try
            let result = mail.DeleteOut a.token letter.MailId
            Assert.Fail()
        with
        | exn -> ()
        