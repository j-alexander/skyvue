namespace SkyVue.Data

type TokenKey = Schema.ServiceTypes.Tokenkey

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Tokenkey =
    let id (x : TokenKey) = x.TokenKeyId
    let tokenId (x : TokenKey) = x.TokenId
    let channelId (x : TokenKey) = x.ChannelId