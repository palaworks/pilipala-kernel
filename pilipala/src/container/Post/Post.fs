namespace pilipala.container.Post

open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open pilipala.pipeline.post

type Post(postId: u64) =

    let mut = MutPost(postId)

    member self.postId = postId

    /// 创建时间
    member self.ctime = mut.ctime

    /// 访问时间
    member self.atime = mut.atime
    /// 修改时间
    member self.mtime = mut.mtime
    /// 访问数
    member self.view = mut.view
    /// 星星数
    member self.star = mut.star
    /// 封面
    member self.cover = mut.cover
    /// 标题
    member self.title = mut.title
    /// 概述
    member self.summary = mut.summary
    /// 正文
    member self.body = mut.body

    member self.asMut() = mut
