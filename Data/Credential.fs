namespace SkyVue.Data

type Credential = Schema.ServiceTypes.Credential

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Credential =
    let id (x : Credential) = x.UserId
    let password (x : Credential) = x.Password