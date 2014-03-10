namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type LogonService(db : DataStore) =

    let validate (identity:string) (password:string) =
        // verify input
        if String.IsNullOrWhiteSpace(identity) then
            raise (BadIdentityOrPassword "You must supply an identity.")
        if String.IsNullOrWhiteSpace(password) || String.length password < 4 then
            raise (BadIdentityOrPassword "You must supply a password.")

        // TODO: encode password and configure identity case
        // verify that a matching user exists
        let matches = query {
            for u in db.User do
            where (u.Identity = identity && u.Password = password)
            select u
        }
        if 1 <> Seq.length matches then
            raise (BadIdentityOrPassword "Check your credentials are entered properly.")

        // retrieve the matching user
        let user = Seq.head matches

        // verify the user is not locked out
        if DataStore.yes <> user.CanLogon then
            raise (AccountLockedOut "Your account was locked out.")

        user

    interface ILogon with

        member x.Authenticate (identity:string) (password:string) =
            // validate the credentials
            let user = validate identity password

            // create and issue a new token
            let token = new Token()
            token.TokenId <- DataStore.id()
            token.UserId <- user.UserId
            token.IsService <- DataStore.yes
            token.DateIssued <- DateTime.Now
            token.DateUsed <- DateTime.Now
            db.Token.InsertOnSubmit(token)
            db.DataContext.SubmitChanges()
            token.TokenId

        member x.ChangePassword (token:string) (previous:string) (password:string) =
            // validate the credentials
            let logon = x :> ILogon
            let user = logon.Verify token
            if (user.Password <> previous) then
                raise (BadIdentityOrPassword "You must enter your previous password correctly.")
          
            // TODO: encode the passwords
            // verify the input
            if String.IsNullOrWhiteSpace(password) || String.length password < 4 then
                raise (BadIdentityOrPassword "You must supply a password.")

            // update the user's password
            user.Password <- password
            db.DataContext.Refresh(RefreshMode.KeepChanges, user)
            db.DataContext.SubmitChanges()
            user

        member x.Logon (identity:string) (password:string) =
            // validate the credentials
            let user = validate identity password
            user.DateOfLogoff <- user.DateAccessed
            user.DateAccessed <- Nullable DateTime.Now
            user.DateOfLogon <- Nullable DateTime.Now
            
            // create and issue a new token
            let token = new Token()
            token.TokenId <- DataStore.id()
            token.UserId <- user.UserId
            token.IsService <- DataStore.no
            token.DateIssued <- DateTime.Now
            token.DateUsed <- DateTime.Now
            db.Token.InsertOnSubmit(token)
            db.DataContext.Refresh(RefreshMode.KeepChanges, user)
            db.DataContext.SubmitChanges()
            token.TokenId

        member x.Verify (token:string) =
            // query for valid tokens
            let expiration = DateTime.Now - TimeSpan.FromHours(1.0)
            let matches = query {
                for t in db.Token do
                where (t.TokenId = token && t.DateUsed > expiration)
                select t
            }
            if (Seq.length matches <> 1) then
                raise (AccessDenied "Your logon token is invalid or has expired.")

            // obtain the token and user
            let token = Seq.head matches
            let user = query {
                for u in db.User do
                where (u.UserId = token.UserId)
                select u
                head
            }
            
            // update the timestamps
            token.DateUsed <- DateTime.Now
            if DataStore.no = token.IsService then
                user.DateAccessed <- Nullable DateTime.Now
            db.DataContext.Refresh(RefreshMode.KeepChanges, token)
            db.DataContext.Refresh(RefreshMode.KeepChanges, user)
            db.DataContext.SubmitChanges()
            user
                