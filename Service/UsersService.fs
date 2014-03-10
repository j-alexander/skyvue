namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type UsersService(db : DataStore) =

    interface IUsers with
        
        member x.List (size:int) (page:int) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes)
                sortBy u.Identity
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.ListSince (size:int) (page:int) (since:DateTime) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes &&
                       u.DateAccessed ?>= since)
                sortBy u.Identity
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.FindByName (name:string) (size:int) (page:int) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes &&
                       SqlMethods.Like(u.Name, name+"%"))
                sortBy u.Name
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.FindByNameSince (name:string) (size:int) (page:int) (since:DateTime) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes &&
                       u.DateAccessed ?>= since &&
                       SqlMethods.Like(u.Name, name+"%"))
                sortBy u.Name
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.FindByIdentity (identity:string) (size:int) (page:int) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes &&
                       SqlMethods.Like(u.Identity, identity+"%"))
                sortBy u.Identity
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.FindByIdentitySince (identity:string) (size:int) (page:int) (since:DateTime) =
            let matches = query {
                for u in db.User do
                where (u.IsListed = DataStore.yes &&
                       u.DateAccessed ?>= since &&
                       SqlMethods.Like(u.Identity, identity+"%"))
                sortBy u.Identity
                skip (size * page)
                take (size)
            }
            Seq.toList matches

        member x.View (userId:string) =
            let matches = query {
                for u in db.User do
                where (u.UserId = userId)
                select u
            }
            if Seq.length matches <> 1 then
                raise (NotFound "A user with that identifier could not be found.")
            Seq.head matches