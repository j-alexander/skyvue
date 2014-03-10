namespace SkyVue.Interface

open SkyVue.Data

// The mail service enables users to send letters to each other. Mail can only
// be sent using accounts that are logged in. External programs should create an
// account in order to send messages to users.
type IMail =

    // Count the number of unread letters in the inbox.
    abstract CountUnread : token:string -> int

    // Count the number of unread letters in a user's inbox. This can be used by
    // anyone, and may be useful for statistics.
    abstract CountUnreadForUser : userId:string -> int

    // Delete mail from the inbox.
    abstract DeleteIn : token:string -> mailId:string -> Mail

    // Delete mail from the outbox.
    abstract DeleteOut : token:string -> mailId:string -> Mail

    // Flag mail in the inbox.
    abstract FlagIn : token:string -> mailId:string -> flagged:bool -> Mail

    // Flag mail in the outbox.
    abstract FlagOut : token:string -> mailId:string -> flagged:bool -> Mail

    // List flagged mail in the inbox.
    abstract ListFlaggedIn : token:string -> size:int -> page:int -> List<Mail>

    // List flagged mail in the outbox.
    abstract ListFlaggedOut : token:string -> size:int -> page:int -> List<Mail>

    // List mail in the inbox.
    abstract ListIn : token:string -> size:int -> page:int -> List<Mail>

    // List mail in the outbox.
    abstract ListOut : token:string -> size:int -> page:int -> List<Mail>

    // List unread mail in the inbox.
    abstract ListUnread : token:string -> size:int -> page:int -> List<Mail>

    // Mark mail as read in the inbox.
    abstract ReadIn : token:string -> mailId:string -> read:bool -> Mail
    
    // Send mail to system users.
    abstract Send : token:string -> subject:string -> contents:string -> userId:seq<string> -> Mail

    // View a mail item in the inbox.
    abstract ViewIn : token:string -> mailId:string -> Mail

    // View a mail item in the outbox.
    abstract ViewOut : token:string -> mailId:string -> Mail