[<AutoOpen>]
module pilipala.container.Comment.ext

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open DbManaged
open DbManaged.PgSql
open DbManaged.PgSql.ext.String
open pilipala.id
open pilipala.db
open pilipala.util
open pilipala.container
open pilipala.container.Post


type public comment_entry with

    /// 创建评论
    /// 返回评论id
    static member create() =
        let table = tables.comment

        let sql =
            $"INSERT INTO {table} \
                    ( commentId,  ownerMetaId,  replyTo,  nick,  content,  email,  site,  ctime) \
                    VALUES \
                    (<commentId>,<ownerMetaId>,<replyTo>,<nick>,<content>,<email>,<site>,<ctime>)"
            |> normalizeSql

        let commentId = palaflake.Next()

        let paras: (string * obj) list =
            [ ("commentId", commentId)
              ("ownerMetaId", 0)
              ("replyTo", 0)
              ("nick", "")
              ("content", "")
              ("email", "")
              ("site", "")
              ("ctime", DateTime.Now) ]

        let aff =
            mkCmd()
                .query(sql, paras)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok commentId
        else
            Err FailedToCreateCommentException


    /// 回收评论
    static member recycle(commentId: u64) =
        let table = tables.comment

        //将评论归属到 0 号元下
        let set = ("ownerMetaId", 0UL)
        let where = ("commentId", commentId)

        let aff =
            mkCmd()
                .update(table, set, where)
                .whenEq(1)
                .executeQuery ()
        //TODO 应该有个异常
        aff |> eq 1 |> ignore |> Ok



    /// 抹除评论
    static member erase(commentId: u64) =
        let table = tables.comment

        let aff =
            mkCmd()
                .delete(table, "commentId", commentId)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToEraseCommentException

    /// 检查Id合法性
    static member check(commentId: u64) =
        let table = tables.comment

        let sql =
            $"SELECT COUNT(*) FROM {table} WHERE commentId = <commentId>"
            |> normalizeSql

        let paras = [ ("commentId", commentId) ]

        let count =
            mkCmd().getFstVal(sql, paras).executeQuery ()
            |> unwrap

        count <> 0

type Post with

    /// 评论
    member self.Comments() =
        let table = tables.comment

        //按时间排序
        let sql =
            $"SELECT commentId FROM {table} WHERE ownerMetaId = <ownerMetaId> \
              ORDER BY ctime"
            |> normalizeSql

        let paras: (string * obj) list = [ ("ownerMetaId", self.postId) ]

        mkCmd().getFstCol(sql, paras).executeQuery ()
        |> map (fun (r: obj) -> Comment(downcast r))
        |> Ok
