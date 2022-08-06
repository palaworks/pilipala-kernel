[<AutoOpen>]
module pilipala.container.post.ext

open fsharper.op
open fsharper.typ
open pilipala.container.comment

type Post with

    /// 文章的用户名
    /// 此功能需UserName插件支持
    member self.UserName: Result'<string, _> =
        if self.CanRead then
            Ok(self.["UserName"].unwrap().coerce ())
        else
            Err "Permission denied"

    /// 文章的评论
    /// 此功能需PostComments插件支持
    member self.Comments: Result'<IMappedComment seq, _> =
        if self.CanRead then
            Ok(self.["Comments"].unwrap().coerce ())
        else
            Err "Permission denied"
