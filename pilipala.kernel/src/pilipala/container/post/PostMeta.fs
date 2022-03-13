namespace pilipala.container.post

open System
open MySql.Data.MySqlClient
open fsharper.op
open fsharper.types
open fsharper.types.Ord
open pilipala
open pilipala.util
open pilipala.container


//映射容器
//对该容器的更改会立即反映到持久化层次
type internal PostMeta(metaId: uint64) =
    //该数据结构用于存放文章元数据

    let fromCache key = cache.get "meta" metaId key
    let intoCache key value = cache.set "meta" metaId key value


    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwarpOr
        <| fun _ ->
            schema.tables
            >>= fun ts ->
                    let table = ts.meta

                    let sql =
                        $"SELECT {key} FROM {table} WHERE metaId = ?metaId"

                    let para = [| MySqlParameter("metaId", metaId) |]

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
                let table = ts.meta

                (table, (key, value), ("metaId", metaId))
                |> schema.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCache
                |> Some
        |> unwarp


    /// 元id
    member this.metaId = metaId
    /// 上级元id
    member this.superMetaId
        with get (): uint64 = this.get "superMetaId"
        and set (v: uint64) = (this.set "superMetaId" v).unwarp ()
    /// 当前记录id
    member this.currRecordId
        with get (): uint64 = this.get "currRecordId"
        and set (v: uint64) = (this.set "currRecordId" v).unwarp ()
    /// 创建时间
    member this.ctime
        with get (): DateTime = this.get "ctime"
        and set (v: DateTime) = (this.set "ctime" v).unwarp ()
    /// 访问时间
    member this.atime
        with get (): DateTime = this.get "atime"
        and set (v: DateTime) = (this.set "atime" v).unwarp ()
    /// 访问数
    member this.view
        with get (): uint32 = this.get "view"
        and set (v: uint32) = (this.set "view" v).unwarp ()
    /// 星星数
    member this.star
        with get (): uint32 = this.get "star"
        and set (v: uint32) = (this.set "star" v).unwarp ()

type PostMeta with

    /// 创建文章元
    /// 返回文章元id
    static member create() =
        schema.tables
        >>= fun ts ->
                let table = ts.meta

                let sql =
                    $"INSERT INTO {table} \
                    ( metaId, superMetaId, currRecordId, ctime, atime, view, star) \
                    VALUES \
                    (?metaId,?superMetaId,?currRecordId,?ctime,?atime,?view,?star)"

                let metaId = palaflake.gen ()

                let recordId = 0 //初始元空

                let para =
                    [| MySqlParameter("metaId", metaId)
                       MySqlParameter("superMetaId", 0)
                       MySqlParameter("currRecordId", recordId)
                       MySqlParameter("ctime", DateTime.Now)
                       MySqlParameter("atime", DateTime.Now)
                       MySqlParameter("view", 0)
                       MySqlParameter("star", 0) |]

                schema.Managed().execute (sql, para)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok metaId
                        | _ -> Err FailedToCreateMeta
                |> unwarp
                |> Some

    /// 抹除文章元
    static member erase(metaId: uint64) =
        schema.tables
        >>= fun ts ->
                let table = ts.meta

                schema.Managed().executeDelete table ("metaId", metaId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseMeta
                |> Some
        |> unwarp
