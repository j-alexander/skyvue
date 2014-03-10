namespace SkyVue.Test

open System
open System.Linq
open NUnit.Framework

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface
open SkyVue.Service

[<TestFixture>]
type ChannelTest() =

    let passworda = "a123"
    let passwordb = "b321"
    let passwordc = "c456"

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
    member x.CreateAndAlter() =
        ()