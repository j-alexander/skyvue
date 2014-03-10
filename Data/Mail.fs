namespace SkyVue.Data

type Mail = Schema.ServiceTypes.Mail

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Mail =
    let id (x : Mail) = x.MailId
    let senderId (x : Mail) = x.SenderId
    let dateSent (x : Mail) = x.DateSent
    let subject (x : Mail) = x.Subject
    let contents (x : Mail) = x.Contents