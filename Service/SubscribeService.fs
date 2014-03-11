namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type SubscribeService(db : DataStore, logon : ILogon) =

    interface ISubscribe with
    
        member x.Access (token:string) (channelId:string) =
            let user = logon.Verify token
            let matches = query {
                for c in db.Channel do
                join s in db.Subscription on (c.ChannelId = s.ChannelId)
                where (s.UserId = user.UserId &&
                       c.ChannelId = channelId)
                select c
            }
            if Seq.length matches <> 1 then
                raise (NotFound "This subscription could not be found.")
            Seq.head matches
    
        member x.Activate (token:string) (channelId:string) (active:bool) =
            let user = logon.Verify token
            let matches = query {
                for c in db.Channel do
                join s in db.Subscription on (c.ChannelId = s.ChannelId)
                where (s.UserId = user.UserId &&
                       c.ChannelId = channelId)
                select s
            }
            if Seq.length matches <> 1 then
                raise (NotFound "This subscription could not be found.")

            let subscription = Seq.head matches
            subscription.IsActive <- if active then DataStore.yes else DataStore.no
            db.DataContext.SubmitChanges()
            subscription

        member x.JoinPublic (token:string) (channelId:string) =
            let user = logon.Verify token
            let matches = query {
                for c in db.Channel do
                where (c.IsPublic = DataStore.yes)
                select c
            }
            if Seq.length matches <> 1 then
                raise (NotFound "A public channel could not be found.")
            let channel = Seq.head matches

            let subscription =
                let matches = query {
                    for s in db.Subscription do
                    where (s.ChannelId = channelId &&
                           s.UserId = user.UserId)
                    select s
                }
                if Seq.length matches <> 1 then
                    let subscription = new Subscription()
                    db.Subscription.InsertOnSubmit(subscription)
                    subscription
                else
                    Seq.head matches

            subscription.CanAdd <-
                if DataStore.yes = subscription.CanAdd ||
                   DataStore.yes = channel.CanAdd then
                   DataStore.yes else DataStore.no
            subscription.CanView <-
                if DataStore.yes = subscription.CanView ||
                   DataStore.yes = channel.CanView then
                   DataStore.yes else DataStore.no
            subscription.CanDelete <-
                if DataStore.yes = subscription.CanDelete ||
                   DataStore.yes = channel.CanDelete then
                   DataStore.yes else DataStore.no
            subscription.IsActive <- DataStore.yes
            db.DataContext.SubmitChanges()
            subscription

        member x.ListAllPublicChannels () =
            let matches = query {
                for c in db.Channel do
                where (c.IsPublic = DataStore.yes)
                sortBy c.Name
                select c
            }
            List.ofSeq matches

        member x.ListPublicChannels (token:string) =
            let user = logon.Verify token
            let matches = query {
                for c in db.Channel do
                where (not (query {
                    for s in db.Subscription do
                    exists (s.ChannelId = c.ChannelId &&
                            s.UserId = user.UserId)
                }))
                select c
            }
            List.ofSeq matches

        member x.ListSubscriptions (token:string) =
            let user = logon.Verify token
            let matches = query {
                for s in db.Subscription do
                where (s.UserId = user.UserId)
                select s
            }
            List.ofSeq matches

        member x.ListUserChannels (token:string) =
            let user = logon.Verify token
            let matches = query {
                for s in db.Subscription do
                join c in db.Channel  on (s.ChannelId = c.ChannelId)
                where (s.UserId = user.UserId)
                sortBy c.Name
                select c
            }
            List.ofSeq matches
    
        member x.Review (token:string) (channelId:string) =
            let user = logon.Verify token
            let matches = query {
                for s in db.Subscription do
                join c in db.Channel on (s.ChannelId = c.ChannelId)
                where (s.UserId = user.UserId &&
                       s.ChannelId = channelId)
                select s
            }
            if Seq.length matches <> 1 then
                raise (NotFound "Your subscription could not be found.")
            Seq.head matches

        member x.View (token:string) (subscriptionId:string) =
            let user = logon.Verify token
            let matches = query {
                for s in db.Subscription do
                where (s.SubscriptionId = subscriptionId &&
                       s.UserId = user.UserId)
                select s
            }
            if Seq.length matches <> 1 then
                raise (NotFound "Your subscription could not be found.")
            Seq.head matches

        member x.Unlock (token:string) (channelId:string) (password:string) =
            let user = logon.Verify token

            // check for an existing key
            let existingKeys = query {
                for k in db.Tokenkey do
                where (k.ChannelId = channelId &&
                       k.TokenId = token)
                select k
            }
            if Seq.length existingKeys = 1 then
                // use the existing key
                Seq.head existingKeys
            else
                // attempt to create a new key
                let matches = query {
                    for c in db.Channel do
                    join l in db.Lock on (c.ChannelId = l.ChannelId)
                    where (c.ChannelId = channelId &&
                           l.ProtectionKey = password)
                    select c
                }
                if Seq.length matches <> 1 then
                    raise (AccessDenied "Unable to match your key to this channel.")
                else
                    let tokenKey = new TokenKey()
                    tokenKey.TokenKeyId <- DataStore.id()
                    tokenKey.ChannelId <- channelId
                    tokenKey.TokenId <- token
                    db.Tokenkey.InsertOnSubmit(tokenKey)
                    db.DataContext.SubmitChanges()
                    tokenKey
                     


        member x.Unsubscribe (token:string) (userId:string) (channelId:string) =
            let user = logon.Verify token
            let matches = query {
                for c in db.Channel do
                join s in db.Subscription on (c.ChannelId = s.ChannelId)
                where (s.ChannelId = channelId &&
                       s.UserId = user.UserId &&
                       s.UserId <> c.UserId)
                select s
            }
            if Seq.length matches <> 1 then
                raise (NotFound "Unable to find your subscription.")

            let subscription = Seq.head matches
            db.Subscription.DeleteOnSubmit(subscription)
            db.DataContext.SubmitChanges()
            subscription