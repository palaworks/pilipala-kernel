namespace pilipala.container.Post

open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open pilipala.pipeline.post

type Post(postId: u64) =

    let mut = MutPost(postId)

    //构建管道并处理值（无意义符号，仅用于代码生成）
    let (-->) v (p: Ref<Pipe<_>>) = p.Value.build().invoke v

    member self.postId = postId

    /// 创建时间
    member self.ctime = mut.ctime --> ctime

    /// 访问时间
    member self.atime = mut.atime --> atime
    /// 修改时间
    member self.mtime = mut.mtime --> mtime
    /// 访问数
    member self.view = mut.view --> view
    /// 星星数
    member self.star = mut.star --> star
    /// 封面
    member self.cover = mut.cover --> cover
    /// 标题
    member self.title = mut.title --> title
    /// 概述
    member self.summary = mut.summary --> summary
    /// 正文
    member self.body = mut.body --> body

    member self.asMut() = mut
