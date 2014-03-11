open Nancy
open Nancy.Hosting.Self
open System

[<EntryPoint>]
let main argv = 

    let path = "http://localhost:1234"
    let host = new NancyHost(new Uri(path))
    host.Start()

    printfn " SkyVue.Host @ %s" path
    printfn " Press enter to terminate."

    Console.ReadLine() |> ignore

    host.Stop()

    0
