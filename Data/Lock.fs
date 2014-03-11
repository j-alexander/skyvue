namespace SkyVue.Data

type Lock = Schema.ServiceTypes.Lock

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Lock =
    let id (x : Lock) = x.ChannelId
    let password (x : Lock) = x.ProtectionKey