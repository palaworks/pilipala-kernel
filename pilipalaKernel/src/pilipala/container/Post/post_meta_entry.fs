namespace pilipala.container.Post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.util
open pilipala.container
open DbManaged.PgSql.ext.String

type post_meta_entry internal (metaId: u64) =

    let fromCache key = cache.get "meta" metaId key
    let intoCache key value = cache.set "meta" metaId key value

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

                            intoCache key value //写入缓存并返回
                            value |> coerce |> Ok
        |> unwrap


    /// 写字段值
    member inline private self.set key value =
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
        |> unwrap

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
    member self.atime
        with get (): DateTime = self.get "atime"
        and set (v: DateTime) = self.set "atime" v
    /// 访问数
    member self.view
        with get (): u32 = self.get "view"
        and set (v: u32) = self.set "view" v
    /// 星星数
    member self.star
        with get (): u32 = self.get "star"
        and set (v: u32) = self.set "star" v
