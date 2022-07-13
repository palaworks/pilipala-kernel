[<AutoOpen>]
module internal pilipala.container.Post.ext

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord

open fsharper.op.Alias
open DbManaged
open DbManaged.PgSql

open pilipala.id
open pilipala.db
open pilipala.util
open pilipala.util.hash
open pilipala.container

type Post with

    /// 根据文章正文生成md5
    member self.md5 =
        (self.cover + self.title + self.summary + self.body)
            .md5

type IPostRecordEntry with

    /// 创建文章记录
    /// 返回文章记录id
    static member create() =
        let table = tables.record

        let sql =
            $"INSERT INTO {table} \
                        ( recordId,  cover,  title,  summary,  body,  mtime) \
                        VALUES \
                        (<recordId>,<cover>,<title>,<summary>,<body>,<mtime>)"
            |> dp.managed.normalizeSql

        let recordId = palaflake.Next()

        let paras: (string * obj) list =
            [ ("recordId", recordId)
              ("cover", "")
              ("title", "")
              ("summary", "")
              ("body", "")
              ("mtime", DateTime.Now) ]

        let aff =
            mkCmd()
                .query(sql, paras)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok recordId
        else
            Err FailedToCreateRecordException

    /// 抹除文章记录
    static member erase(recordId: u64) =
        let table = tables.record

        let aff =
            mkCmd()
                .delete(table, "recordId", recordId)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToEraseRecordException

    /// 检查Id合法性
    static member check(metaId: u64) =
        let table = tables.meta

        let sql =
            $"SELECT COUNT(*) FROM {table} WHERE metaId = <metaId>"
            |> dp.managed.normalizeSql

        let paras = [ ("metaId", metaId) ]

        let count =
            mkCmd()
                .getFstVal(sql, paras)
                .executeQuery()
                .unwrap ()

        count <> 0

type IPostMetaEntry with

    static member create() =
        let table = tables.meta

        let sql =
            $"INSERT INTO {table} \
                    ( metaId,  baseMetaId,  bindRecordId,  ctime,  atime,  view,  star) \
                    VALUES \
                    (<metaId>,<baseMetaId>,<bindRecordId>,<ctime>,<atime>,<view>,<star>)"
            |> dp.managed.normalizeSql

        let metaId = palaflake.Next()

        let recordId = 0 //初始元空

        let paras: (string * obj) list =
            [ ("metaId", metaId)
              ("baseMetaId", 0)
              ("bindRecordId", recordId)
              ("ctime", DateTime.Now)
              ("atime", DateTime.Now)
              ("view", 0)
              ("star", 0) ]

        let aff =
            mkCmd()
                .query(sql, paras)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok metaId
        else
            Err FailedToCreateMetaException

    /// 抹除文章元
    static member erase(metaId: u64) : Result'<unit, exn> =
        let table = tables.meta

        let aff =
            mkCmd()
                .delete(table, "metaId", metaId)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToEraseMetaException


    /// 检查Id合法性
    static member check(recordId: u64) =
        let table = tables.record

        let sql =
            $"SELECT COUNT(*) FROM {table} WHERE recordId = <recordId>"
            |> dp.managed.normalizeSql

        let paras = [ ("recordId", recordId) ]


        let count =
            mkCmd()
                .getFstVal(sql, paras)
                .executeQuery()
                .unwrap ()

        count <> 0
