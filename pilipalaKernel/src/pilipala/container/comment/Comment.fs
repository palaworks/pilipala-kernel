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
        (fromCache key).unwrapOr
        <| fun _ ->
            db.tables
            >>= fun ts ->
                    let table = ts.comment

                    let sql =
                        $"SELECT {key} FROM {table} WHERE commentId = ?commentId"

                    let para =
                        [| MySqlParameter("commentId", commentId) |]

                    db.Managed().getFstVal (sql, para)
                    >>= fun r ->
                            let value = r.unwrap ()

                            intoCache key value //写入缓存并返回
                            value |> Ok

                    |> unwrap
                    |> coerce
                    |> Some
            |> unwrap



    /// 写字段值
    member inline private this.set key value =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                (table, (key, value), ("commentId", commentId))
                |> db.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCache
                |> Some
        |> unwrap

    /// 评论id
    member this.commentId = commentId
    /// 所属元id
    member this.ownerMetaId
        with get (): uint64 = this.get "ownerMetaId"
        and set (v: uint64) = (this.set "ownerMetaId" v).unwrap ()
    /// 回复到
    member this.replyTo
        with get (): uint64 = this.get "replyTo"
        and set (v: uint64) = (this.set "replyTo" v).unwrap ()
    /// 昵称
    member this.nick
        with get (): string = this.get "nick"
        and set (v: string) = (this.set "nick" v).unwrap ()
    /// 内容
    member this.content
        with get (): string = this.get "content"
        and set (v: string) = (this.set "content" v).unwrap ()
    /// 电子邮箱
    member this.email
        with get (): string = this.get "email"
        and set (v: string) = (this.set "email" v).unwrap ()
    /// 站点
    member this.site
        with get (): string = this.get "site"
        and set (v: string) = (this.set "site" v).unwrap ()
    /// 创建时间
    member this.ctime
        with get (): DateTime = this.get "ctime"
        and set (v: DateTime) = (this.set "ctime" v).unwrap ()

type public Comment with

    /// 创建评论
    /// 返回评论id
    static member create() =
        db.tables
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

                db.Managed().executeAny (sql, para)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok commentId
                        | _ -> Err FailedToCreateComment
                |> unwrap
                |> Some

    /// 回收评论
    static member recycle(commentId: uint64) =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                //将评论归属到 0 号元下
                let set = ("ownerMetaId", 0UL)
                let where = ("commentId", commentId)

                db.Managed().executeUpdate (table, set, where)
                >>= fun f -> eq 1 |> f |> ignore |> Ok
                |> Some
        |> unwrap


    /// 抹除评论
    static member erase(commentId: uint64) =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                db.Managed().executeDelete table ("commentId", commentId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseComment
                |> Some
        |> unwrap

module ext =

    type Post with

        /// 评论
        member self.Comments() =
            db.tables
            >>= fun ts ->
                    let table = ts.comment

                    //按时间排序
                    let sql =
                        $"SELECT commentId FROM {table} WHERE ownerMetaId = ?ownerMetaId \
                          ORDER BY ctime"

                    let paras = [ ("ownerMetaId", self.id) ]

                    db.Managed().getCol (sql, 0u, paras)
                    >>= fun rows ->

                            match rows with
                            | None -> Ok []
                            | Some rows' ->
                                Ok
                                <| map (fun (r: obj) -> Comment(downcast r)) rows'
                    |> unwrap
                    |> Some
