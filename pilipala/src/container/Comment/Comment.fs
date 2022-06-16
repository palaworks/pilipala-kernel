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

type Comment(commentId: u64) =

    let mut = MutComment(commentId)

    /// 评论id
    member self.commentId = commentId
    /// 所属元id
    member self.ownerMetaId = mut.ownerMetaId
    /// 回复到
    member self.replyTo = mut.replyTo
    /// 昵称
    member self.nick = mut.nick
    /// 内容
    member self.content = mut.content
    /// 电子邮箱
    member self.email = mut.email
    /// 站点
    member self.site = mut.site
    /// 创建时间
    member self.ctime = mut.ctime

    member self.asMut() = mut
