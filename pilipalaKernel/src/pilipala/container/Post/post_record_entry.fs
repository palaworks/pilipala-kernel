namespace pilipala.container.Post

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

type post_record_entry internal (recordId: u64) =

    let fromCache key = cache.get "record" recordId key
    let intoCache key value = cache.set "record" recordId key value

    /// 取字段值
    member inline private self.get key =
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
    member inline private self.set key value =
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
    member self.recordId = recordId
    /// 封面
    member self.cover
        with get (): string = self.get "cover"
        and set (v: string) = self.set "cover" v
    /// 标题
    member self.title
        with get (): string = self.get "title"
        and set (v: string) = self.set "title" v
    /// 概述
    member self.summary
        with get (): string = self.get "summary"
        and set (v: string) = self.set "summary" v
    /// 正文
    member self.body
        with get (): string = self.get "body"
        and set (v: string) = self.set "body" v
    /// 修改时间
    member self.mtime
        with get (): DateTime = self.get "mtime"
        and set (v: DateTime) = self.set "mtime" v
