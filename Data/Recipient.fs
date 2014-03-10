namespace SkyVue.Data

type Recipient = Schema.ServiceTypes.Recipient

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Recipient =
    let mailId (x : Recipient) = x.MailId
    let userId (x : Recipient) = x.UserId