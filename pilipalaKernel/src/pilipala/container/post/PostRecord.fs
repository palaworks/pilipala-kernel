﻿namespace pilipala.container.post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open pilipala
open pilipala.util
open pilipala.util.hash
open pilipala.container
open DbManaged.PgSql.ext.String

//映射型容器
//对该容器的更改会立即反映到持久化层次
type PostRecord internal (recordId: uint64) =

    let fromCache key = cache.get "record" recordId key
    let intoCache key value = cache.set "record" recordId key value

    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwrapOr
        <| fun _ ->
            db.tables
            >>= fun ts ->
                    let key = ""
                    let table = ts.record

                    let sql =
                        $"SELECT {key} FROM {table} WHERE recordId = <recordId>"
                        |> normalizeSql

                    let paras: (string * obj) list = [ ("recordId", recordId) ]


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
                let table = ts.record

                (table, (key, value), ("recordId", recordId))
                |> db.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCacheException
        |> unwrap


    /// 记录id
    member this.recordId = recordId
    /// 封面
    member this.cover
        with get (): string = this.get "cover"
        and set (v: string) = this.set "cover" v
    /// 标题
    member this.title
        with get (): string = this.get "title"
        and set (v: string) = this.set "title" v
    /// 概述
    member this.summary
        with get (): string = this.get "summary"
        and set (v: string) = this.set "summary" v
    /// 正文
    member this.body
        with get (): string = this.get "body"
        and set (v: string) = this.set "body" v
    /// 修改时间
    member this.mtime
        with get (): DateTime = this.get "mtime"
        and set (v: DateTime) = this.set "mtime" v

type PostRecord with

    /// 根据文章主要正文生成md5
    member self.md5 =
        (self.cover + self.title + self.summary + self.body)
            .md5

    /// 以正文为参数执行闭包 f, 常用于概述为空时取得一个替代值
    member self.trySummary f = f self.body

type PostRecord with

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
    static member erase(recordId: uint64) =
        db.tables
        >>= fun ts ->
                let table = ts.record

                db.Managed().executeDelete table ("recordId", recordId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseRecordException
