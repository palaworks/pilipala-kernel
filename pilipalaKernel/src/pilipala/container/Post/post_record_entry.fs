namespace pilipala.container.Post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.taskQueue
open pilipala.container.cache
open pilipala.container
open DbManaged.PgSql.ext.String

type post_record_entry internal (recordId: u64) =

    let cache =
        ContainerCacheHandler(db.tables.record, "recordId", recordId)

    /// 记录id
    member self.recordId = recordId
    /// 封面
    member self.cover
        with get (): string = cache.get "cover"
        and set (v: string) = cache.set "cover" v
    /// 标题
    member self.title
        with get (): string = cache.get "title"
        and set (v: string) = cache.set "title" v
    /// 概述
    member self.summary
        with get (): string = cache.get "summary"
        and set (v: string) = cache.set "summary" v
    /// 正文
    member self.body
        with get (): string = cache.get "body"
        and set (v: string) = cache.set "body" v
    /// 修改时间
    member self.mtime
        with get (): DateTime = cache.get "mtime"
        and set (v: DateTime) = cache.set "mtime" v
