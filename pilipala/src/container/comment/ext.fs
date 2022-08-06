[<AutoOpen>]
module pilipala.container.comment.ext

open fsharper.op
open fsharper.typ

type Comment with

    /// 评论的用户名
    /// 此功能需UserName插件支持
    member self.UserName: Result'<string, _> =
        if self.CanRead then
            Ok(self.["UserName"].unwrap().coerce ())
        else
            Err "Permission denied"

    /// 评论的回复
    /// 此功能需CommentReplies插件支持
    member self.Replies: Result'<IMappedComment seq, _> =
        if self.CanRead then
            Ok(self.["Replies"].unwrap().coerce ())
        else
            Err "Permission denied"
