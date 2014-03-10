namespace SkyVue.Data

open System
open Microsoft.FSharp.Data.TypeProviders

type Schema = SqlDataConnection<"Data Source=localhost\SQLEXPRESS;Initial Catalog=skyvue;Integrated Security=SSPI;">


type DataStore = Schema.ServiceTypes.SimpleDataContextTypes.Skyvue

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DataStore =
    let yes = 1s
    let no = 0s
    let id() = Guid.NewGuid().ToString()