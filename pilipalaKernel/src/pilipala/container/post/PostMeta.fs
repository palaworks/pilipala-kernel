namespace pilipala.container.post

open System
open fsharper.op
open fsharper.types
open fsharper.types.Ord
open pilipala
open pilipala.util
open pilipala.container
open DbManaged.PgSql.ext.String


//映射容器
//对该容器的更改会立即反映到持久化层次
type internal PostMeta(metaId: uint64) =
    //该数据结构用于存放文章元数据

    let fromCache key = cache.get "meta" metaId key
    let intoCache key value = cache.set "meta" metaId key value


    /// 取字段值
    member inline private this.get key =
        (fromCache key).unwrapOr
        <| fun _ ->
            db.tables
            >>= fun ts ->
                    let table = ts.meta

                    let sql =
                        $"SELECT {key} FROM {table} WHERE metaId = <metaId>"
                        |> normalizeSql

                    let paras: (string * obj) list = [ ("metaId", metaId) ]

                    db.Managed().getFstVal (sql, paras)
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
                let table = ts.meta

                (table, (key, value), ("metaId", metaId))
                |> db.Managed().executeUpdate
                >>= fun f ->

                        //当更改记录数为 1 时才会提交事务并追加到缓存头
                        match f <| eq 1 with
                        | 1 -> Ok <| intoCache key value
                        | _ -> Err FailedToWriteCacheException
                |> Some
        |> unwrap


    /// 元id
    member this.metaId = metaId
    /// 上级元id
    member this.superMetaId
        with get (): uint64 = this.get "baseMetaId"
        and set (v: uint64) = this.set "baseMetaId" v |> unwrap
    /// 当前记录id
    member this.currRecordId
        with get (): uint64 = this.get "bindRecordId"
        and set (v: uint64) = this.set "bindRecordId" v |> unwrap
    /// 创建时间
    member this.ctime
        with get (): DateTime = this.get "ctime"
        and set (v: DateTime) = this.set "ctime" v |> unwrap
    /// 访问时间
    member this.atime
        with get (): DateTime = this.get "atime"
        and set (v: DateTime) = this.set "atime" v |> unwrap
    /// 访问数
    member this.view
        with get (): uint32 = this.get "view"
        and set (v: uint32) = this.set "view" v |> unwrap
    /// 星星数
    member this.star
        with get (): uint32 = this.get "star"
        and set (v: uint32) = this.set "star" v |> unwrap

type PostMeta with

    /// 创建文章元
    /// 返回文章元id
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
                |> Some
        |> unwrap

    /// 抹除文章元
    static member erase(metaId: uint64) =
        db.tables
        >>= fun ts ->
                let table = ts.meta

                db.Managed().executeDelete table ("metaId", metaId)
                >>= fun f ->

                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseMetaException
                |> Some
        |> unwrap
