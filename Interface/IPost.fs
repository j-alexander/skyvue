namespace SkyVue.Interface

open System
open SkyVue.Data
    
// The post service allows users to post messages to a channel that can be read
// by other subscribers.
type IPost =

    // Add a new post.
    abstract Add : token:string -> channelId:string -> subject:string -> contents:string -> PostContent

    // Delete the supplied post.
    abstract Delete : token:string -> channelId:string -> contentId:string -> PostContent

    // Edit the supplied post.
    abstract Edit : token:string -> channelId:string -> contentId:string -> subject:string -> contents:string -> PostContent

    // List channel posts by page between the specified dates.
    abstract List : token:string -> channelId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<PostContent>
    
    // List public channel posts by page between the specified dates.
    abstract ListPublic : channelId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<PostContent>

    // List channel topics by page between the specified dates.
    abstract ListTopics : token:string -> channelId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<TopicContent>

    // List public channel topics by page between the specified dates.
    abstract ListTopicsPublic : channelId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<TopicContent>

    // List channel posts for a topic by page between the specified dates.
    abstract ListTopic : token:string -> channelId:string -> contentId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<PostContent>

    // List public channel posts for a topic by page between the specified dates.
    abstract ListTopicPublic : channelId:string -> contentId:string -> start:DateTime -> finish:DateTime -> size:int -> page:int -> List<PostContent>

    // Determine which page a post is likely located on.
    abstract PageOf : token:string -> channelId:string -> contentId:string -> size:int -> int

    // Determine which page a public post is likely located on.
    abstract PageOfPublic : channelId:string -> contentId:string -> size:int -> int
    
    // Reply to an existing post.
    abstract Reply : token:string -> channelId:string -> contentId:string -> subject:string -> contents:string -> PostContent

    // View a post on the specified channel.
    abstract View : token:string -> channelId:string -> contentId:string -> PostContent

    // View a post on the specified channel.
    abstract ViewPublic : channelId:string -> contentId:string -> PostContent