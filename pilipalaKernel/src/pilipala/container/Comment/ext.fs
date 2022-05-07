module pilipala.container.comment.ext

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open DbManaged.PgSql.ext.String
open pilipala
open pilipala.util
open pilipala.container
open pilipala.container.Post


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
    static member recycle(commentId: u64) =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                //将评论归属到 0 号元下
                let set = ("ownerMetaId", 0UL)
                let where = ("commentId", commentId)

                db.Managed().executeUpdate (table, set, where)
                >>= fun f -> eq 1 |> f |> ignore |> Ok



    /// 抹除评论
    static member erase(commentId: u64) =
        db.tables
        >>= fun ts ->
                let table = ts.comment

                db.Managed().executeDelete table ("commentId", commentId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseCommentException


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

                let paras: (string * obj) list = [ ("ownerMetaId", self.postId) ]

                db.Managed().getCol (sql, 0u, paras)
                >>= fun x ->

                        match x with
                        | None -> []
                        | Some rows -> map (fun (r: obj) -> Comment(downcast r)) rows
                        |> Ok
