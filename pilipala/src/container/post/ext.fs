[<AutoOpen>]
module pilipala.container.post.ext

open pilipala.container.comment

type IPost with

    /// 文章的评论
    /// 此功能需PostComments插件支持
    member self.Comments: IComment seq =
        downcast self.["Comments"].unwrap ()
