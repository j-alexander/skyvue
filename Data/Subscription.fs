namespace SkyVue.Data

type Subscription = Schema.ServiceTypes.Subscription

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Subscription =
    let id (x : Subscription) = x.SubscriptionId
    let channelId (x : Subscription) = x.ChannelId
    let userId (x : Subscription) = x.UserId
    let canDelete (x : Subscription) = x.CanDelete
    let canAdd (x : Subscription) = x.CanAdd
    let canView (x : Subscription) = x.CanView
    let isActive (x : Subscription) = x.IsActive
    
    let permissions (x : Subscription) =
        Set.empty
        |> Permission.augment Add x.CanAdd
        |> Permission.augment View x.CanView
        |> Permission.augment Delete x.CanDelete