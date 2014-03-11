namespace SkyVue.Data

type User = Schema.ServiceTypes.User

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module User =
    let id (x : User) = x.UserId
    let canLogon (x : User) = x.CanLogon
    let isListed (x : User) = x.IsListed
    let dateAccessed (x : User) = x.DateAccessed
    let dateCreated (x : User) = x.DateCreated
    let dateOfLogoff (x : User) = x.DateOfLogoff
    let dateOfLogon (x : User) = x.DateOfLogon
    let eMail (x : User) = x.EMail
    let identity (x : User) = x.Identity
    let name (x : User) = x.Name