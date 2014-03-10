namespace SkyVue.Data

type File = Schema.ServiceTypes.File

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module File =
    let id (x : File) = x.ContentId
    let description (x : File) = x.Description
    let mime (x : File) = x.Mime
    let name (x : File) = x.Name
    let size (x : File) = x.Size