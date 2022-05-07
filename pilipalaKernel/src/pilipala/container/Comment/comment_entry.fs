namespace pilipala.container.Comment

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.util
open pilipala.container
open DbManaged.PgSql.ext.String

type comment_entry internal (commentId: u64) =

    let fromCache key = cache.get "comment" commentId key
    let intoCache key value = cache.set "comment" commentId key value

    /// 取字段值
    member inline private self.get key =
        (fromCache key).unwrapOr
        <| fun _ ->
            db.tables
            >>= fun ts ->
                    let table = ts.comment

                    let sql =
                        $"SELECT {key} FROM {table} WHERE commentId = <commentId>"
                        |> normalizeSql

                    let paras: (string * obj) list = [ ("commentId", commentId) ]

                    db.Managed().getFstVal (sql, paras)
                    >>= fun r ->
                            let value = r.unwrap ()

                            intoCache key value //写入缓存并返回
                            value |> coerce |> Ok
        |> unwrap

    /// 写字段值
    member inline private self.set key value =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                (table, (key, value), ("commentId", commentId))
                |> db.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCacheException

        |> unwrap

    /// 评论id
    member self.commentId = commentId
    /// 所属元id
    member self.ownerMetaId
        with get (): u64 = self.get "ownerMetaId"
        and set (v: u64) = self.set "ownerMetaId" v
    /// 回复到
    member self.replyTo
        with get (): u64 = self.get "replyTo"
        and set (v: u64) = self.set "replyTo" v
    /// 昵称
    member self.nick
        with get (): string = self.get "nick"
        and set (v: string) = self.set "nick" v
    /// 内容
    member self.content
        with get (): string = self.get "content"
        and set (v: string) = self.set "content" v
    /// 电子邮箱
    member self.email
        with get (): string = self.get "email"
        and set (v: string) = self.set "email" v
    /// 站点
    member self.site
        with get (): string = self.get "site"
        and set (v: string) = self.set "site" v
    /// 创建时间
    member self.ctime
        with get (): DateTime = self.get "ctime"
        and set (v: DateTime) = self.set "ctime" v
