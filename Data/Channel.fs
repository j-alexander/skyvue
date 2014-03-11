namespace SkyVue.Data

type Channel = Schema.ServiceTypes.Channel

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Channel =
    let id (x : Channel) = x.ChannelId
    let canAdd (x : Channel) = x.CanAdd
    let canDelete (x : Channel) = x.CanDelete
    let canView (x : Channel) = x.CanView
    let icon (x : Channel) = x.Icon
    let isProtected (x : Channel) = x.IsProtected
    let isPublic (x : Channel) = x.IsPublic
    let name (x : Channel) = x.Name
    let userId (x : Channel) = x.UserId

    let permissions (x : Channel) =
        if DataStore.yes = x.IsPublic then
            Set.empty
            |> Permission.augment Add x.CanAdd
            |> Permission.augment View x.CanView
            |> Permission.augment Delete x.CanDelete
        else
            Set.empty


            