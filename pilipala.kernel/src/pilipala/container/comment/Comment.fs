namespace pilipala.container.comment

open System
open MySql.Data.MySqlClient
open fsharper.op
open fsharper.types
open fsharper.types.Ord
open pilipala
open pilipala.util
open pilipala.container
open pilipala.container.post


type Comment(commentId: uint64) =

    let fromCache key = cache.get "comment" commentId key

    let intoCache key value = cache.set "comment" commentId key value

    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwarpOr
        <| fun _ ->
            schema.tables
            >>= fun ts ->
                    let table = ts.comment

                    let sql =
                        $"SELECT {key} FROM {table} WHERE commentId = ?commentId"

                    let para =
                        [| MySqlParameter("commentId", commentId) |]

                    schema.Managed().getFstVal (sql, para)
                    >>= fun r ->
                            let value = r.unwarp ()

                            intoCache key value //写入缓存并返回
                            value |> Ok

                    |> unwarp
                    |> coerce
                    |> Some
            |> unwarp



    /// 写字段值
    member inline private this.set key value =
        schema.tables
        >>= fun ts ->
                let table = ts.comment

                (table, (key, value), ("commentId", commentId))
                |> schema.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCache
                |> Some
        |> unwarp

    /// 评论id
    member this.commentId = commentId
    /// 所属元id
    member this.ownerMetaId
        with get (): uint64 = this.get "ownerMetaId"
        and set (v: uint64) = (this.set "ownerMetaId" v).unwarp ()
    /// 回复到
    member this.replyTo
        with get (): uint64 = this.get "replyTo"
        and set (v: uint64) = (this.set "replyTo" v).unwarp ()
    /// 昵称
    member this.nick
        with get (): string = this.get "nick"
        and set (v: string) = (this.set "nick" v).unwarp ()
    /// 内容
    member this.content
        with get (): string = this.get "content"
        and set (v: string) = (this.set "content" v).unwarp ()
    /// 电子邮箱
    member this.email
        with get (): string = this.get "email"
        and set (v: string) = (this.set "email" v).unwarp ()
    /// 站点
    member this.site
        with get (): string = this.get "site"
        and set (v: string) = (this.set "site" v).unwarp ()
    /// 创建时间
    member this.ctime
        with get (): DateTime = this.get "ctime"
        and set (v: DateTime) = (this.set "ctime" v).unwarp ()

type public Comment with

    /// 创建评论
    /// 返回评论id
    static member create() =
        schema.tables
        >>= fun ts ->
                let table = ts.comment

                let sql =
                    $"INSERT INTO {table} \
                    ( commentId, ownerMetaId, replyTo, nick, content, email, site, ctime) \
                    VALUES \
                    (?commentId,?ownerMetaId,?replyTo,?nick,?content,?email,?site,?ctime)"

                let commentId = palaflake.gen ()

                let para =
                    [| MySqlParameter("commentId", commentId)
                       MySqlParameter("ownerMetaId", 0)
                       MySqlParameter("replyTo", 0)
                       MySqlParameter("nick", "")
                       MySqlParameter("content", "")
                       MySqlParameter("email", "")
                       MySqlParameter("site", "")
                       MySqlParameter("ctime", DateTime.Now) |]

                schema.Managed().execute (sql, para)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok commentId
                        | _ -> Err FailedToCreateComment
                |> unwarp
                |> Some

    /// 回收评论
    static member recycle(commentId: uint64) =
        schema.tables
        >>= fun ts ->
                let table = ts.comment

                //将评论归属到 0 号元下
                let set = ("ownerMetaId", 0UL)
                let where = ("commentId", commentId)

                schema.Managed().executeUpdate (table, set, where)
                >>= fun f -> eq 1 |> f |> ignore |> Ok
                |> Some
        |> unwarp


    /// 抹除评论
    static member erase(commentId: uint64) =
        schema.tables
        >>= fun ts ->
                let table = ts.comment

                schema.Managed().executeDelete table ("commentId", commentId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseComment
                |> Some
        |> unwarp

module ext =

    type Post with

        /// 评论
        member self.Comments() =
            schema.tables
            >>= fun ts ->
                    let table = ts.comment

                    //按时间排序
                    let sql =
                        $"SELECT commentId FROM {table} WHERE ownerMetaId = ?ownerMetaId \
                          ORDER BY ctime"

                    let para =
                        [| MySqlParameter("ownerMetaId", self.id) |]

                    schema.Managed().getFstCol (sql, para)
                    >>= fun rows ->

                            match rows with
                            | None -> Ok []
                            | Some rows' ->
                                Ok
                                <| map (fun (r: obj) -> Comment(downcast r)) rows'
                    |> unwarp
                    |> Some
