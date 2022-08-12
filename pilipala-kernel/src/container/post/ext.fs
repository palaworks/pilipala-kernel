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
            self.["UserName"]
            >>= fun x -> x.unwrap().coerce () |> Ok
        else
            Err "Permission denied"
