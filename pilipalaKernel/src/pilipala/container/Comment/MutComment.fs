namespace pilipala.container.Comment

open fsharper.op.Alias
open pilipala.pipeline
open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
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

type MutComment internal (commentId: u64) =

    let entry = comment_entry (commentId)

    /// 评论id
    member self.commentId = commentId
    /// 所属元id
    member self.ownerMetaId
        with get (): u64 = entry.ownerMetaId
        and set (v: u64) = entry.ownerMetaId <- v
    /// 回复到
    member self.replyTo
        with get (): u64 = entry.replyTo
        and set (v: u64) = entry.replyTo <- v
    /// 昵称
    member self.nick
        with get (): string = entry.nick
        and set (v: string) = entry.nick <- v
    /// 内容
    member self.content
        with get (): string = entry.content
        and set (v: string) = entry.content <- v
    /// 电子邮箱
    member self.email
        with get (): string = entry.email
        and set (v: string) = entry.email <- v
    /// 站点
    member self.site
        with get (): string = entry.site
        and set (v: string) = entry.site <- v
    /// 创建时间
    member self.ctime
        with get (): DateTime = entry.ctime
        and set (v: DateTime) = entry.ctime <- v
