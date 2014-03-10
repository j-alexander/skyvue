namespace SkyVue.Data

type Token = Schema.ServiceTypes.Token

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Token =
    let id (x : Token) = x.TokenId
    let userId (x : Token) = x.UserId
    let dateIssued (x : Token) = x.DateIssued
    let dateUsed (x : Token) = x.DateUsed
    let isService (x : Token) = x.IsService