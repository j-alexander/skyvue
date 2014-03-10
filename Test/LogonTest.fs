namespace SkyVue.Test

open System
open System.Linq
open NUnit.Framework

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface
open SkyVue.Service

[<TestFixture>]
type LogonTest() =

    let password1 = "a123"
    let password2 = "a234"
    let passwordb = "b321"
    let passwordc = "c456"
    let date = DateTime.Now

    [<SetUp>]
    member x.SetUp() =
        Database.create()
        let join = new JoinService(Database.connect()) :> IJoin
        let a = join.Join "a" password1 "J1" "a@test.com"
        let b = join.Join "b" passwordb "J2" "b@test.com"
        let c = join.Join "c" passwordc "K1" "c@test.com"
        ()

    [<TearDown>]
    member x.TearDown() =
        Database.delete()

    [<Test>]
    member x.SimpleLoginAndVerify() =
        let logon = new LogonService(Database.connect()) :> ILogon
        let users = new UsersService(Database.connect()) :> IUsers

        let verify (user : User) =
            Assert.AreEqual("a", user.Identity)
            Assert.True(user.DateOfLogon.HasValue)
            Assert.GreaterOrEqual(user.DateOfLogon.Value, date - TimeSpan.FromMinutes 1.0)
            Assert.GreaterOrEqual(date + TimeSpan.FromMinutes 1.0, user.DateOfLogon.Value)
            Assert.True(user.DateAccessed.HasValue)
            Assert.GreaterOrEqual(user.DateAccessed.Value, date - TimeSpan.FromMinutes 1.0)
            Assert.GreaterOrEqual(date + TimeSpan.FromMinutes 1.0, user.DateAccessed.Value)
        
        // logon and check the returned values
        logon.Logon "a" password1 |> logon.Verify |> verify

        // check the dates from the directory
        users.FindByIdentity "a" 1 0 |> List.head |> verify

    [<Test>]
    member x.TestInvalidPassword() =
        let logon = new LogonService(Database.connect()) :> ILogon
        let users = new UsersService(Database.connect()) :> IUsers

        let verify x =
            try
                let user = logon.Logon "a" x
                Assert.Fail()
            with
            | :? BadIdentityOrPassword ->
                let user = users.FindByIdentity "a" 1 0 |> List.head
                Assert.False(user.DateOfLogon.HasValue)
                Assert.False(user.DateAccessed.HasValue)
            | _ -> Assert.Fail()

        verify password2
        verify passwordb
        verify passwordc
        verify "a12"
        verify "a1234"
        verify ""
        verify null
                
    [<Test>]
    member x.TestChangePassword() =
        let logon = new LogonService(Database.connect()) :> ILogon

        let initialToken = logon.Logon "a" password1
        let initialUser = logon.Verify initialToken
        let updatedUser = logon.ChangePassword initialToken password1 password2
        let updatedToken = logon.Logon "a" password2
        try
            let user = logon.Logon "a" password1
            Assert.Fail()
        with
        | :? BadIdentityOrPassword -> ()
        | _ -> Assert.Fail()

        
    [<Test>]
    member x.AuthenticateAndVerify() =
        let logon = new LogonService(Database.connect()) :> ILogon
        let users = new UsersService(Database.connect()) :> IUsers

        let verify (user : User) =
            Assert.AreEqual("a", user.Identity)
            Assert.False(user.DateOfLogon.HasValue)
            Assert.False(user.DateAccessed.HasValue)
        
        // authenticate and check the returned values
        logon.Authenticate "a" password1 |> logon.Verify |> verify
        
        // authenticate again and check the returned values
        logon.Authenticate "a" password1 |> logon.Verify |> verify

        // check the dates from the directory
        users.FindByIdentity "a" 1 0 |> List.head |> verify