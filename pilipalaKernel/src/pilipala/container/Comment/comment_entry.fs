namespace pilipala.container.Comment

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

type comment_entry internal (commentId: u64) =
 
    let cache =
        ContainerCacheHandler(db.tables.unwrap().comment, "commentId", commentId)

    /// 评论id
    member self.commentId = commentId
    /// 所属元id
    member self.ownerMetaId
        with get (): u64 = cache.get "ownerMetaId"
        and set (v: u64) = cache.set "ownerMetaId" v
    /// 回复到
    member self.replyTo
        with get (): u64 = cache.get "replyTo"
        and set (v: u64) = cache.set "replyTo" v
    /// 昵称
    member self.nick
        with get (): string = cache.get "nick"
        and set (v: string) = cache.set "nick" v
    /// 内容
    member self.content
        with get (): string = cache.get "content"
        and set (v: string) = cache.set "content" v
    /// 电子邮箱
    member self.email
        with get (): string = cache.get "email"
        and set (v: string) = cache.set "email" v
    /// 站点
    member self.site
        with get (): string = cache.get "site"
        and set (v: string) = cache.set "site" v
    /// 创建时间
    member self.ctime
        with get (): DateTime = cache.get "ctime"
        and set (v: DateTime) = cache.set "ctime" v
