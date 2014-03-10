namespace SkyVue.Data

type Content = Schema.ServiceTypes.Content

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Content =
    let id (x : Content) = x.ContentId
    let channelId (x : Content) = x.ChannelId
    let userId (x : Content) = x.UserId
    let dateCreated (x : Content) = x.DateCreated
    let dateModified (x : Content) = x.DateModified
    let isSummarized (x : Content) = x.IsSummarized