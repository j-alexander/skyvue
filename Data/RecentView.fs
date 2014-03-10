namespace SkyVue.Data

type RecentView = Schema.ServiceTypes.Recentview

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module RecentView =
    let id (x : RecentView) = x.RecentViewId
    let dateViewed (x : RecentView) = x.DateViewed
    let contentId (x : RecentView) = x.ContentId
    let tokenId (x : RecentView) = x.TokenId