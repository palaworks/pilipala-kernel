module internal pilipala.container.Post.ext

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open pilipala
open pilipala.util
open pilipala.container
open DbManaged.PgSql.ext.String
open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.util
open pilipala.util.hash
open pilipala.container
open DbManaged.PgSql.ext.String

type record with

    /// 根据文章主要正文生成md5
    member self.md5 =
        (self.cover + self.title + self.summary + self.body)
            .md5

    /// 以正文为参数执行闭包 f, 常用于概述为空时取得一个替代值
    member self.trySummary f = f self.body

type record with

    /// 创建文章记录
    /// 返回文章记录id
    static member create() =
        db.tables
        >>= fun ts ->
                let table = ts.record

                let sql =
                    $"INSERT INTO {table} \
                        ( recordId,  cover,  title,  summary,  body,  mtime) \
                        VALUES \
                        (<recordId>,<cover>,<title>,<summary>,<body>,<mtime>)"
                    |> normalizeSql

                let recordId = palaflake.gen ()

                let paras: (string * obj) list =
                    [ ("recordId", recordId)
                      ("cover", "")
                      ("title", "")
                      ("summary", "")
                      ("body", "")
                      ("mtime", DateTime.Now) ]

                db.Managed().executeAny (sql, paras)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok recordId
                        | _ -> Err FailedToCreateRecordException

    /// 抹除文章记录
    static member erase(recordId: u64) =
        db.tables
        >>= fun ts ->
                let table = ts.record

                db.Managed().executeDelete table ("recordId", recordId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseRecordException

type meta with

    static member create() =
        db.tables
        >>= fun ts ->
                let table = ts.meta

                let sql =
                    $"INSERT INTO {table} \
                    ( metaId,  baseMetaId,  bindRecordId,  ctime,  atime,  view,  star) \
                    VALUES \
                    (<metaId>,<baseMetaId>,<bindRecordId>,<ctime>,<atime>,<view>,<star>)"
                    |> normalizeSql

                let metaId = palaflake.gen ()

                let recordId = 0 //初始元空

                let paras: (string * obj) list =
                    [ ("metaId", metaId)
                      ("baseMetaId", 0)
                      ("bindRecordId", recordId)
                      ("ctime", DateTime.Now)
                      ("atime", DateTime.Now)
                      ("view", 0)
                      ("star", 0) ]

                db.Managed().executeAny (sql, paras)
                >>= fun f ->
                        match f <| eq 1 with
                        | 1 -> Ok metaId
                        | _ -> Err FailedToCreateMetaException

    /// 抹除文章元
    static member erase(metaId: u64) : Result'<unit, exn> =
        db.tables
        >>= fun ts ->
                let table = ts.meta

                db.Managed().executeDelete table ("metaId", metaId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseMetaException
