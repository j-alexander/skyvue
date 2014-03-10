namespace SkyVue.Data

type Topic = Schema.ServiceTypes.Topic
type TopicContent = Topic * Content

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Topic =
    let id (x : Topic) = x.ContentId
    let subject (x : Topic) = x.Subject