namespace pilipala.container.Comment

open System
open fsharper.op.Alias

type ICommentEntry =
    /// 评论id
    abstract commentId : u64
    /// 所属元id
    abstract ownerMetaId : u64 with get, set
    /// 回复到
    abstract replyTo : u64 with get, set
    /// 昵称
    abstract nick : string with get, set
    /// 内容
    abstract content : string with get, set
    /// 电子邮箱
    abstract email : string with get, set
    /// 站点
    abstract site : string with get, set
    /// 创建时间
    abstract ctime : DateTime with get, set
