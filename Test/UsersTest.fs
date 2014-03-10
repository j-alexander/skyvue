namespace SkyVue.Test

open System
open System.Linq
open NUnit.Framework

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface
open SkyVue.Service

[<TestFixture>]
type UsersTest() =

    let password = "test_password"

    [<SetUp>]
    member x.SetUp() =
        Database.create()
        let join = new JoinService(Database.connect()) :> IJoin
        let a = join.Join "a" password "J1" "a@test.com"
        let b = join.Join "b" password "J2" "b@test.com"
        let c = join.Join "c" password "K1" "c@test.com"
        ()

    [<TearDown>]
    member x.TearDown() =
        Database.delete()

    [<Test>]
    member x.TestSimpleList() =
        let users = new UsersService(Database.connect()) :> IUsers

        match users.List 10 0 |> List.map User.identity with
        | ["a"; "b"; "c"] -> () | _ -> Assert.Fail()
         
    [<Test>]
    member x.TestSimpleListPaging() =
        let users = new UsersService(Database.connect()) :> IUsers
        
        match users.List 2 0 |> List.map User.identity with
        | ["a"; "b"] -> () | _ ->  Assert.Fail()

        match users.List 2 1 |> List.map User.identity with
        | "c" :: [] -> () | _ ->  Assert.Fail()

        match users.List 2 2 |> List.map User.identity with
        | [] -> () |  _ ->  Assert.Fail()

    [<Test>]
    member x.TestFindByName() =
        let users = new UsersService(Database.connect()) :> IUsers
        
        match users.FindByName "J" 10 0 |> List.map User.name with
        | ["J1"; "J2"] -> () | _ -> Assert.Fail()

        match users.FindByName "K" 10 0 |> List.map User.name with
        | "K1" :: [] -> () | _ -> Assert.Fail()

        match users.FindByName "J" 1 0 |> List.map User.name with
        | "J1" :: [] -> () | _ -> Assert.Fail()

        match users.FindByName "J" 1 1 |> List.map User.name with
        | "J2" :: [] -> () | _ -> Assert.Fail()

        match users.FindByName "J" 1 2 |> List.map User.name with
        | [] -> () | _ -> Assert.Fail()