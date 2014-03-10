namespace SkyVue.Data

type Inbox = Schema.ServiceTypes.Inbox

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Inbox =
    let id (x : Inbox) = x.InboxId
    let mailId (x : Inbox) = x.MailId
    let userId (x : Inbox) = x.UserId
    let isFlagged (x : Inbox) = x.IsFlagged
    let isRead (x : Inbox) = x.IsRead