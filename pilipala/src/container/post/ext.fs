[<AutoOpen>]
module pilipala.container.post.ext

open System
open fsharper.op
open fsharper.typ
open pilipala.access.user
open pilipala.container.comment

type IMappedPost with

    /// 文章的用户名
    /// 此功能需UserName插件支持
    member self.UserName: string =
        downcast self.["UserName"].unwrap ()

    /// 文章的评论
    /// 此功能需PostComments插件支持
    member self.Comments: IMappedComment seq =
        downcast self.["Comments"].unwrap ()
