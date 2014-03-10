namespace SkyVue.Data

type Outbox = Schema.ServiceTypes.Outbox

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Outbox =
    let id (x : Outbox) = x.OutboxId
    let mailId (x : Outbox) = x.MailId
    let userId (x : Outbox) = x.UserId
    let isFlagged (x : Outbox) = x.IsFlagged