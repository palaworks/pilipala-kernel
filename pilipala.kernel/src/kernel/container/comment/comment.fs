namespace pilipala.container.comment

open System
open MySql.Data.MySqlClient
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open pilipala.util
open pilipala.launcher
open pilipala.container
open pilipala.container.post
open pilipala.container.cache


type Comment(commentId: uint64) =

    let fromCache key = KVPool.get "comment" commentId key

    let intoCache key value =
        KVPool.set "comment" commentId key value

    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwarpOr
        <| fun _ ->
            unwarp (
                pala ()
                >>= fun p ->
                        let table = p.table.comment

                        let sql =
                            $"SELECT {key} FROM {table} WHERE commentId = ?commentId"

                        let para =
                            [| MySqlParameter("commentId", commentId) |]

                        p.database.getFstVal (sql, para)
                        >>= fun r ->
                                let value = r.unwarp ()

                                intoCache key value //写入缓存并返回
                                value |> cast |> Ok
            )


    /// 写字段值
    member inline private this.set key value =
        pala ()
        >>= fun p ->
                let table = p.table.comment

                (table, (key, value), ("commentId", commentId))
                |> p.database.executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCache


    /// 评论id
    member this.commentId = commentId
    /// 所属栈id
    member this.ownerStackId
        with get (): uint64 = this.get "ownerStackId"
        and set (v: uint64) = (this.set "ownerStackId" v).unwarp ()
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

type Comment with

    /// 创建评论
    /// 返回评论id
    static member create() =
        pala ()
        >>= fun p ->
                let table = p.table.comment

                let sql =
                    $"INSERT INTO {table} \
                    ( commentId, ownerStackId, replyTo, nick, content, email, site, ctime) \
                    VALUES \
                    (?commentId,?ownerStackId,?replyTo,?nick,?content,?email,?site,?ctime)"

                let commentId = palaflake.gen ()

                let para =
                    [| MySqlParameter("commentId", commentId)
                       MySqlParameter("ownerStackId", 0)
                       MySqlParameter("replyTo", 0)
                       MySqlParameter("nick", "")
                       MySqlParameter("content", "")
                       MySqlParameter("email", "")
                       MySqlParameter("site", "")
                       MySqlParameter("ctime", DateTime.Now) |]

                p.database.execute (sql, para)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok commentId
                        | _ -> Err FailedToCreateComment

    /// 回收评论
    static member recycle(commentId: uint64) =
        pala ()
        >>= fun p ->
                let table = p.table.comment

                //将评论归属到 0 号栈下
                let set = ("ownerStackId", 0UL)
                let where = ("commentId", commentId)

                p.database.executeUpdate (table, set, where)
                >>= fun f -> eq 1 |> f |> ignore |> Ok


    /// 抹除评论
    static member erase(commentId: uint64) =
        pala ()
        >>= fun p ->
                let table = p.table.comment

                p.database.executeDelete table ("commentId", commentId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseComment

module ext =

    type PostStack with

        /// 评论
        member self.comments =
            pala ()
            >>= fun p ->
                    let table = p.table.comment

                    //按时间排序
                    let sql =
                        $"SELECT commentId FROM {table} WHERE ownerStackId = ?ownerStackId \
                          ORDER BY ctime"

                    let para =
                        [| MySqlParameter("ownerStackId", self.stackId) |]

                    p.database.getFstCol (sql, para)
                    >>= fun rows ->

                            match rows with
                            | None -> Ok []
                            | Some rows' ->
                                Ok
                                <| map (fun (r: obj) -> Comment(downcast r)) rows'
