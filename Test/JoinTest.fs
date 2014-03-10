namespace SkyVue.Test

open System
open NUnit.Framework

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface
open SkyVue.Service

[<TestFixture>]
type JoinTest() =

    let identity = "test_identity"
    let password = "test_password"
    let name = "test_name"
    let email = "test_user@server.com"
    let created = DateTime.Now

    [<SetUp>]
    member x.SetUp() = Database.create()

    [<TearDown>]
    member x.TearDown() = Database.delete()

    [<Test>]
    member x.TestCreateUser() =
        let db = Database.connect()
        let join = new JoinService(db) :> IJoin
        let users = new UsersService(db) :> IUsers

        let verify (user : User) =
            Assert.AreEqual(identity, user.Identity)
            Assert.AreEqual(password, user.Password)
            Assert.AreEqual(name, user.Name)
            Assert.AreEqual(email, user.EMail)
            Assert.GreaterOrEqual(user.DateCreated, created - TimeSpan.FromMinutes 1.0)
            Assert.GreaterOrEqual(DateTime.Now + TimeSpan.FromMinutes 1.0, user.DateCreated)

        // insert the user and verify the result
        verify (join.Join identity password name email)
        // verify the directory result
        verify (List.head (users.List 10 0))

    [<Test>]
    member x.TestInvalidIdentity() =
        let db = Database.connect()
        let join = new JoinService(db) :> IJoin
        let users = new UsersService(db) :> IUsers

        let attempt (badIdentity : string) =
            try
                let user = join.Join badIdentity password name email
                Assert.Fail("An exception should be thrown.")
            with
            | :? BadIdentityOrPassword -> ()
            | _ ->
                Assert.Fail("Unexpected exception type.")

        // verify that some common checks work
        attempt ""
        attempt null
        // double-check that no entries were inserted
        Assert.AreEqual(0, List.length (users.List 10 0))

    [<Test>]
    member x.TestInvalidPassword() =
        let db = Database.connect()
        let join = new JoinService(db) :> IJoin
        let users = new UsersService(db) :> IUsers

        let attempt (badPassword : string) =
            try
                let user = join.Join identity badPassword name email
                Assert.Fail("An exception should be thrown.")
            with
            | :? BadIdentityOrPassword -> ()
            | _ ->
                Assert.Fail("Unexpected exception type.")

        // verify that some common checks work
        attempt ""
        attempt null
        attempt "a"
        attempt "ab"
        attempt "abc"
        // double-check that no entries were inserted
        Assert.AreEqual(0, List.length (users.List 10 0))

    [<Test>]
    member x.TestInvalidEmail() =
        let db = Database.connect()
        let join = new JoinService(db) :> IJoin
        let users = new UsersService(db) :> IUsers

        let attempt (badEmail : string) =
            try
                let user = join.Join identity password name badEmail
                Assert.Fail("An exception should be thrown.")
            with
            | :? BadIdentityOrPassword -> ()
            | _ ->
                Assert.Fail("Unexpected exception type.")

        // verify that some common checks work
        attempt "simplename"
        attempt "simplenamewithoutdomain@"
        attempt "@domainbyitself.com"
        attempt "@domainbyitself"
        attempt "with @ spaces .com"
        // double-check that no entries were inserted
        Assert.AreEqual(0, List.length (users.List 10 0))
        
    [<Test>]
    member x.TestDuplicateIdentity() =
        let db = Database.connect()
        let join = new JoinService(db) :> IJoin
        let users = new UsersService(db) :> IUsers

        // insert the first user
        let user = join.Join identity password name email
        
        // verify that the duplicate fails 
        try
            let user = join.Join identity password name email
            Assert.Fail("An exception should be thrown.")
        with
        | :? AlreadyExists -> ()
        | _ ->
            Assert.Fail("Unexpected exception type.")
            
        // double-check that only 1 entry was inserted
        Assert.AreEqual(1, List.length (users.List 10 0))