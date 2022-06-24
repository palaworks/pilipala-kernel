namespace pilipala.container.Comment

open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open pilipala.pipeline.comment

type Comment(commentId: u64) =

    let mut = MutComment(commentId)

    //构建管道并处理值（无意义符号，仅用于代码生成）
    let (-->) v (p: Ref<Pipe<_>>) = p.Value.build().invoke v

    /// 评论id
    member self.commentId = commentId
    /// 所属元id
    member self.ownerMetaId = mut.ownerMetaId
    /// 回复到
    member self.replyTo = mut.replyTo
    /// 昵称
    member self.nick = mut.nick --> nick
    /// 内容
    member self.content = mut.content --> content
    /// 电子邮箱
    member self.email = mut.email --> email
    /// 站点
    member self.site = mut.site --> site
    /// 创建时间
    member self.ctime = mut.ctime --> ctime

    member self.asMut() = mut
