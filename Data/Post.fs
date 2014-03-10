namespace SkyVue.Data

type Post = Schema.ServiceTypes.Post
type PostContent = Post * Content

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Post =
    let id (x : Post) = x.ContentId
    let contents (x : Post) = x.Contents
    let parentId (x : Post) = x.ParentId
    let topicId (x : Post) = x.TopicId