namespace pilipala.container.Post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.taskQueue
open pilipala.container
open DbManaged.PgSql.ext.String

type post_meta_entry internal (metaId: u64) =

    let fromCache key = cache.get metaId key
    let intoCache key value = cache.set metaId key value

    let rmCache key = cache.rm metaId key //清除无效缓存

    /// 取字段值
    member inline private self.get key =
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

                            //写入缓存并返回
                            intoCache key value
                            value |> coerce |> Ok
        |> unwrap


    /// 写字段值
    member inline private self.set key value =

        intoCache key value //先加入缓存

        fun _ ->
            db.tables
            >>= fun ts ->
                    let table = ts.meta

                    (table, (key, value), ("metaId", metaId))
                    |> db.Managed().executeUpdate
                    >>= fun f ->

                            //当更改记录数为 1 时才会提交事务
                            match f <| eq 1 with
                            | 1 -> Ok()
                            | _ ->
                                rmCache key //清除无效缓存
                                Err FailedToWriteCacheException
        |> queueTask

    /// 元信息id
    member self.metaId = metaId


    /// 上级元信息id
    member self.baseMetaId
        with get (): u64 = self.get "baseMetaId"
        and set (v: u64) = self.set "baseMetaId" v
    /// 绑定记录id
    member self.bindRecordId
        with get (): u64 = self.get "bindRecordId"
        and set (v: u64) = self.set "bindRecordId" v
    /// 创建时间
    member self.ctime
        with get (): DateTime = self.get "ctime"
        and set (v: DateTime) = self.set "ctime" v


    /// 访问时间
    /// 可延迟持久化
    member self.atime
        with get (): DateTime = self.get "atime"
        and set (v: DateTime) = self.set "atime" v
    /// 访问数
    /// 可延迟持久化
    member self.view
        with get (): u32 = self.get "view"
        and set (v: u32) = self.set "view" v
    /// 星星数
    /// 可延迟持久化
    member self.star
        with get (): u32 = self.get "star"
        and set (v: u32) = self.set "star" v
