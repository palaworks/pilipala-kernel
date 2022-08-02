[<AutoOpen>]
module pilipala.container.comment.ext

type IComment with

    /// 评论的用户名
    /// 此功能需UserName插件支持
    member self.UserName: string =
        downcast self.["UserName"].unwrap ()

    /// 评论的回复
    /// 此功能需CommentReplies插件支持
    member self.Replies: IComment seq =
        downcast self.["Replies"].unwrap ()
