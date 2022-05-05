namespace pilipala.container.comment

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open pilipala
open pilipala.util
open pilipala.container
open pilipala.container.post
open DbManaged.PgSql.ext.String


type Comment internal (commentId: uint64) =

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
                        | _ -> Err FailedToWriteCacheException

        |> unwrap

    /// 评论id
    member this.commentId = commentId
    /// 所属元id
    member this.ownerMetaId
        with get (): uint64 = this.get "ownerMetaId"
        and set (v: uint64) = this.set "ownerMetaId" v
    /// 回复到
    member this.replyTo
        with get (): uint64 = this.get "replyTo"
        and set (v: uint64) = this.set "replyTo" v
    /// 昵称
    member this.nick
        with get (): string = this.get "nick"
        and set (v: string) = this.set "nick" v
    /// 内容
    member this.content
        with get (): string = this.get "content"
        and set (v: string) = this.set "content" v
    /// 电子邮箱
    member this.email
        with get (): string = this.get "email"
        and set (v: string) = this.set "email" v
    /// 站点
    member this.site
        with get (): string = this.get "site"
        and set (v: string) = this.set "site" v
    /// 创建时间
    member this.ctime
        with get (): DateTime = this.get "ctime"
        and set (v: DateTime) = this.set "ctime" v

type public Comment with

    /// 创建评论
    /// 返回评论id
    static member create() =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                let sql =
                    $"INSERT INTO {table} \
                    ( commentId,  ownerMetaId,  replyTo,  nick,  content,  email,  site,  ctime) \
                    VALUES \
                    (<commentId>,<ownerMetaId>,<replyTo>,<nick>,<content>,<email>,<site>,<ctime>)"
                    |> normalizeSql

                let commentId = palaflake.gen ()

                let paras: (string * obj) list =
                    [ ("commentId", commentId)
                      ("ownerMetaId", 0)
                      ("replyTo", 0)
                      ("nick", "")
                      ("content", "")
                      ("email", "")
                      ("site", "")
                      ("ctime", DateTime.Now) ]

                db.Managed().executeAny (sql, paras)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok commentId
                        | _ -> Err FailedToCreateCommentException


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



    /// 抹除评论
    static member erase(commentId: uint64) =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                db.Managed().executeDelete table ("commentId", commentId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseCommentException


module ext =

    type Post with

        /// 评论
        member self.Comments() =
            db.tables
            >>= fun ts ->
                    let table = ts.comment

                    //按时间排序
                    let sql =
                        $"SELECT commentId FROM {table} WHERE ownerMetaId = <ownerMetaId> \
                          ORDER BY ctime"
                        |> normalizeSql

                    let paras: (string * obj) list = [ ("ownerMetaId", self.id) ]

                    db.Managed().getCol (sql, 0u, paras)
                    >>= fun x ->

                            match x with
                            | None -> []
                            | Some rows -> map (fun (r: obj) -> Comment(downcast r)) rows
                            |> Ok
