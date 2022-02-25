﻿namespace pilipala.container.post

open System
open MySql.Data.MySqlClient
open fsharper.fn
open fsharper.op
open fsharper.ethType.ethOption
open fsharper.ethType.ethResult
open fsharper.typeExt
open fsharper.moreType
open pilipala
open pilipala.util
open pilipala.util.hash
open pilipala.container

type PostRecord(recordId: uint64) =

    let fromCache key = cache.get "record" recordId key
    let intoCache key value = cache.set "record" recordId key value



    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwarpOr
        <| fun _ ->
            schema.tables
            >>= fun ts ->
                    let key = ""
                    let table = ts.record

                    let sql =
                        $"SELECT {key} FROM {table} WHERE recordId = ?recordId"

                    let para =
                        [| MySqlParameter("recordId", recordId) |]


                    schema.Managed().getFstVal (sql, para)
                    >>= fun r ->
                            let value = r.unwarp ()

                            intoCache key value //写入缓存并返回
                            value |> Ok

                    |> unwarp
                    |> cast
                    |> Some
            |> unwarp




    /// 写字段值
    member inline private this.set key value =
        schema.tables
        >>= fun ts ->
                let table = ts.record

                (table, (key, value), ("recordId", recordId))
                |> schema.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCache
                |> Some
        |> unwarp

    /// 记录id
    member this.recordId = recordId
    /// 封面
    member this.cover
        with get (): string = this.get "cover"
        and set (v: string) = (this.set "cover" v).unwarp ()
    /// 标题
    member this.title
        with get (): string = this.get "title"
        and set (v: string) = (this.set "title" v).unwarp ()
    /// 概述
    member this.summary
        with get (): string = this.get "summary"
        and set (v: string) = (this.set "summary" v).unwarp ()
    /// 正文
    member this.body
        with get () = this.get "body"
        and set (v: string) = (this.set "body" v).unwarp ()
    /// 修改时间
    member this.mtime
        with get (): DateTime = this.get "mtime"
        and set (v: DateTime) = (this.set "mtime" v).unwarp ()

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
        schema.tables
        >>= fun ts ->
                let table = ts.record

                let sql =
                    $"INSERT INTO {table} \
                        ( recordId, cover, title, summary, body, mtime) \
                        VALUES \
                        (?recordId,?cover,?title,?summary,?body,?mtime)"

                let recordId = palaflake.gen ()

                let para =
                    [| MySqlParameter("recordId", recordId)
                       MySqlParameter("cover", "")
                       MySqlParameter("title", "")
                       MySqlParameter("summary", "")
                       MySqlParameter("body", "")
                       MySqlParameter("mtime", DateTime.Now) |]

                schema.Managed().execute (sql, para)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok recordId
                        | _ -> Err FailedToCreateRecord
                |> unwarp
                |> Some

    /// 抹除文章记录
    static member erase(recordId: uint64) =
        schema.tables
        >>= fun ts ->
                let table = ts.record

                schema.Managed().executeDelete table ("recordId", recordId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseRecord
                |> Some
        |> unwarp
