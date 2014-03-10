namespace SkyVue.Service

open Microsoft.FSharp.Linq
open System
open System.Data.Linq.SqlClient
open System.Linq
open System.Text.RegularExpressions

open SkyVue.Data
open SkyVue.Error
open SkyVue.Interface

type PostService(db : DataStore, logon : ILogon) =


    let verify (token:string) (channelId:string) (required : List<Permission>) =
        let user = logon.Verify token

        // build the set of available permissions
        let publicPermissions = query {
            for c in db.Channel do
            where (c.IsPublic = DataStore.yes)
            select (Channel.permissions c)
        }
        let userPermissions = query {
            for s in db.Subscription do
            where (s.ChannelId = channelId &&
                   s.UserId = user.UserId)
            select (Subscription.permissions s)
        }
        let available =
            Seq.append publicPermissions userPermissions
            |> Seq.fold Set.union Set.empty
        
        // determine which if any are unmet
        let unmet =  (Set.ofList required) - available
        if not (Set.isEmpty unmet) then
            raise (AccessDenied "You require additional permissions for this action.")

        // return the user and the permissioned channel
        let channel = query {
            for c in db.Channel do
            where (c.ChannelId = channelId)
            head
        }
        user, channel


    interface IPost with

        member x.Add (token:string) (channelId:string) (subject:string) (contents:string) =
            let user, channel = verify token channelId [Add]

            // create the new topic
            let topic, content = new Topic(), new Content()
            content.ContentId <- DataStore.id()
            content.ChannelId <- channel.ChannelId
            content.UserId <- user.UserId
            content.DateCreated <- DateTime.Now
            content.DateModified <- DateTime.Now
            topic.ContentId <- content.ContentId
            topic.Subject <- subject
            db.Content.InsertOnSubmit(content)
            db.Topic.InsertOnSubmit(topic)

            // create the new post
            let post, content = new Post(), new Content()
            content.ContentId <- DataStore.id()
            content.ChannelId <- channel.ChannelId
            content.UserId <- user.UserId
            content.DateCreated <- DateTime.Now
            content.DateModified <- DateTime.Now
            post.ContentId <- content.ContentId
            post.Contents <- contents
            post.TopicId <- topic.ContentId
            db.Content.InsertOnSubmit(content)
            db.Post.InsertOnSubmit(post)
            db.DataContext.SubmitChanges()
            post, content

        member x.Delete (token:string) (channelId:string) (contentId:string) =
            let user, channel = verify token channelId [Delete]

            // find the post to delete
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (p.ContentId = contentId)
                select (p, c)
            }
            if (Seq.length matches <> 1) then
                raise (NotFound "Unable to find the post you've chosen to delete.")
            let post, content = Seq.head matches
            
            // check ownership
            if user.UserId <> content.UserId then
                if user.UserId <> channel.UserId then
                    raise (AccessDenied "You must own the post or channel to delete it.")
            
            // count the number of posts in this topic
            let topicSize = query {
                for t in db.Topic do
                join p in db.Post on (t.ContentId = p.TopicId)
                where (t.ContentId = post.TopicId)
                count
            }

            // mark this post for deletion
            db.Content.DeleteOnSubmit(content)
            db.Post.DeleteOnSubmit(post)

            // move any children up in the tree
            let children = query {
                for p in db.Post do
                where (p.ParentId = contentId)
                select p
            }
            for child in children do
                child.ParentId <- post.ParentId

            // remove any empty topic
            if topicSize <= 1 then
                let topic, content = query {
                    for t in db.Topic do
                    join c in db.Content on (t.ContentId = c.ContentId)
                    where (t.ContentId = post.TopicId)
                    select (t, c)
                    head
                }
                db.Topic.DeleteOnSubmit(topic)
                db.Content.DeleteOnSubmit(content)
            db.DataContext.SubmitChanges()
            post, content

        member x.Edit (token:string) (channelId:string) (contentId:string) (subject:string) (contents:string) =
            let user, channel = verify token channelId [Add; Delete]
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (p.ContentId = contentId &&
                       c.ChannelId = channelId &&
                       c.UserId = user.UserId)
                select (p, c)
            }
            if Seq.length matches <> 1 then
                raise (NotFound "Unable to find a valid post to edit.")
            let post, content = Seq.head matches
            content.DateModified <- DateTime.Now
            post.Contents <- contents
            db.DataContext.SubmitChanges()
            post, content

        member x.List (token:string) (channelId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let user, channel = verify token channelId [View]
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (c.ChannelId = channel.ChannelId &&
                       c.DateModified >= start &&
                       c.DateModified <= finish)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (p, c)
            }
            List.ofSeq matches

    
        member x.ListPublic (channelId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let matches = query {
                for x in db.Channel do
                join c in db.Content on (x.ChannelId = c.ChannelId)
                join p in db.Post on (c.ContentId = p.ContentId)
                where (x.ChannelId = channelId &&
                       x.IsPublic = DataStore.yes &&
                       x.CanView = DataStore.yes &&
                       c.DateModified >= start &&
                       c.DateModified <= finish)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (p, c)
            }
            List.ofSeq matches

        member x.ListTopics (token:string) (channelId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let user, channel = verify token channelId [View]
            let matches = query {
                for c in db.Content do
                join t in db.Topic on (c.ContentId = t.ContentId)
                where (c.ChannelId = channelId &&
                       c.DateModified >= start &&
                       c.DateModified <= finish)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (t, c)
            }
            List.ofSeq matches

        member x.ListTopicsPublic (channelId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let matches = query {
                for x in db.Channel do
                join c in db.Content on (x.ChannelId = c.ChannelId)
                join t in db.Topic on (c.ContentId = t.ContentId)
                where (x.ChannelId = channelId &&
                       x.IsPublic = DataStore.yes &&
                       x.CanView = DataStore.yes &&
                       c.DateModified >= start &&
                       c.DateModified <= finish)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (t, c)
            }
            List.ofSeq matches

        member x.ListTopic (token:string) (channelId:string) (contentId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let user, channel = verify token channelId [View]
            let matches = query {
                for t in db.Topic do
                join p in db.Post on (t.ContentId = p.TopicId)
                join c in db.Content on (p.ContentId = c.ContentId)
                where (t.ContentId = contentId &&
                       c.ChannelId = channel.ChannelId)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (p, c)
            }
            List.ofSeq matches

        member x.ListTopicPublic (channelId:string) (contentId:string) (start:DateTime) (finish:DateTime) (size:int) (page:int) =
            let matches = query {
                for t in db.Topic do
                join p in db.Post on (t.ContentId = p.TopicId)
                join c in db.Content on (p.ContentId = c.ContentId)
                join x in db.Channel on (c.ChannelId = x.ChannelId)
                where (x.ChannelId = channelId &&
                       x.IsPublic = DataStore.yes &&
                       x.CanView = DataStore.yes &&
                       t.ContentId = contentId)
                sortByDescending c.DateModified
                skip (size * page)
                take (size)
                select (p, c)
            }
            List.ofSeq matches

        member x.PageOf (token:string) (channelId:string) (contentId:string) (size:int) =
            let user, channel = verify token channelId [View]
            let newerPosts = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (c.ChannelId = channel.ChannelId &&
                       c.DateModified >= (query {
                            for x in db.Content do
                            where (x.ContentId = contentId &&
                                   x.ChannelId = c.ChannelId)
                            select x.DateModified
                            head
                       }))
                count
            }
            let estimate = Math.Ceiling((float newerPosts) / (float size))
            let page = (int estimate) - 1
            page

        member x.PageOfPublic (channelId:string) (contentId:string) (size:int) =
            let newerPosts = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                join x in db.Channel on (c.ChannelId = x.ChannelId)
                where (c.ChannelId = channelId &&
                       x.IsPublic = DataStore.yes &&
                       x.CanView = DataStore.yes &&
                       c.DateModified >= (query {
                            for y in db.Content do
                            where (y.ContentId = contentId &&
                                   y.ChannelId = c.ChannelId)
                            select y.DateModified
                            head
                       }))
                count
            }
            let estimate = Math.Ceiling((float newerPosts) / (float size))
            let page = (int estimate) - 1
            page
    
        member x.Reply (token:string) (channelId:string) (contentId:string) (subject:string) (contents:string) =
            let user, channel = verify token channelId [Add]
            
            // find the post to reply to
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (p.ContentId = contentId &&
                       c.ChannelId = channel.ChannelId)
                select p
            }
            if (Seq.length matches <> 1) then
                raise (NotFound "Unable to find the post you're replying to.")
            let parent = Seq.head matches
            
            // create the new post
            let post, content = new Post(), new Content()
            content.ContentId <- DataStore.id()
            content.ChannelId <- channel.ChannelId
            content.UserId <- user.UserId
            content.DateCreated <- DateTime.Now
            content.DateModified <- DateTime.Now
            post.ContentId <- content.ContentId
            post.Contents <- contents
            post.TopicId <- parent.TopicId
            db.Content.InsertOnSubmit(content)
            db.Post.InsertOnSubmit(post)
            db.DataContext.SubmitChanges()
            post, content
            

        member x.View (token:string) (channelId:string) (contentId:string) =
            let user, channel = verify token channelId [View]
            
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                where (p.ContentId = contentId &&
                       c.ChannelId = channel.ChannelId)
                select (p,c)
            }
            if (Seq.length matches <> 1) then
                raise (NotFound "Unable to find the post you're seeking.")

            let post, content = Seq.head matches
            post, content

        member x.ViewPublic (channelId:string) (contentId:string) =
            let matches = query {
                for p in db.Post do
                join c in db.Content on (p.ContentId = c.ContentId)
                join x in db.Channel on (c.ChannelId = x.ChannelId)
                where (p.ContentId = contentId &&
                       x.IsPublic = DataStore.yes &&
                       x.CanView = DataStore.yes &&
                       c.ChannelId = channelId)
                select (p,c)
            }
            if (Seq.length matches <> 1) then
                raise (NotFound "Unable to find the post you're seeking.")

            let post, content = Seq.head matches
            post, content