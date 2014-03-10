namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type JoinService(db : DataStore) =

    let isEMail =
        let email =
            Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" +
                  @"@" +
                  @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$")
        (fun x -> email.Match(x).Success)

    interface IJoin with

        member x.Join (identity:string) (password:string) (name:string) (mail:string) =

            // verify input
            if String.IsNullOrWhiteSpace(identity) then
                raise (BadIdentityOrPassword "You must supply an identity.")
            if String.IsNullOrWhiteSpace(password) || String.length password < 4 then
                raise (BadIdentityOrPassword "You must supply a good password.")
            if String.IsNullOrWhiteSpace(name) then
                raise (BadIdentityOrPassword "You must supply your full name.")
            if String.IsNullOrWhiteSpace(mail) then
                raise (BadIdentityOrPassword "You must supply an e-mail address.")
            if not (isEMail mail) then
                raise (BadIdentityOrPassword "You must supply a real e-mail address.")

            // trim the input of whitespace
            let identity, password, name, mail = identity.Trim(), password.Trim(), name.Trim(), mail.Trim()

            // find existing users with that identity
            let existingMatches = query {
                for u in db.User do
                where (u.Identity = identity)
                count
            }
            if existingMatches > 0 then
                raise (AlreadyExists "This identity is already in use.")
               
            // insert and return the new user     
            let user = new User()
            user.UserId <- DataStore.id()
            user.DateCreated <- DateTime.Now
            user.IsListed <- DataStore.yes
            user.CanLogon <- DataStore.yes
            user.Identity <- identity
            user.Name <- name
            user.Password <- password
            user.EMail <- mail
            db.User.InsertOnSubmit(user)
            db.DataContext.SubmitChanges()
            user