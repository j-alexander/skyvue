namespace SkyVue.Interface

open SkyVue.Data

// Configure channel subscriptions.
type ISubscribe =

    // Acquire the channel object corresponding to a subscription.
    abstract Access : token:string -> channelId:string -> Channel
    
    // Users can accept and activate new subscription offers.
    abstract Activate : token:string -> channelId:string -> active:bool -> Subscription

    // Subscribe to a public channel, merge the greater of permissions.
    abstract JoinPublic : token:string -> channelId:string -> Subscription

    // List all public channels.
    abstract ListAllPublicChannels : unit -> List<Channel>

    // List public channels that the user is not subscribed to.
    abstract ListPublicChannels : token:string -> List<Channel>

    // List a user's channel subscriptions, for the user only.
    abstract ListSubscriptions : token:string -> List<Subscription>

    // List a user's channels, for the user only.
    abstract ListUserChannels : token:string -> List<Channel>
    
    // Review your subscription to a channel.
    abstract Review : token:string -> channelId:string -> Subscription

    // View a subscription, as the channel owner or the person subscribed.
    abstract View : token:string -> subscriptionId:string -> Subscription

    // Unlock a protected channel (for the token specified) using the supplied
    // password. The channel is unlocked only for this token during its session
    // lifetime.
    abstract Unlock : token:string -> channelId:string -> password:string -> TokenKey

    // If requested by the channel owner or the user in question, this
    // completely removes the subsciption.
    abstract Unsubscribe : token:string -> userId:string -> channelId:string -> Subscription