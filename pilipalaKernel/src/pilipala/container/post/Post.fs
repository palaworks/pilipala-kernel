namespace pilipala.container.post

open pilipala.pipeline.post

//值容器
//对该容器的更改不会反映到持久化层次
type Post private (meta: PostMeta, record: PostRecord) =
    //惰性求值

    new(postId: uint64) =
        let meta = PostMeta(postId)
        let record = PostRecord(meta.currRecordId)
        Post(meta, record)

    //属性字段惰性求值

    /// Id
    member self.id = meta.metaId
    /// 封面
    member self.cover =
        record.cover |> coverRenderPipeline.build().invoke
    /// 标题
    member self.title =
        record.title |> titleRenderPipeline.build().invoke
    /// 概要
    member self.summary =
        record.summary
        |> summaryRenderPipeline.build().invoke
    /// 正文
    member self.body =
        record.body |> bodyRenderPipeline.build().invoke

    /// 创建时间
    member self.ctime =
        meta.ctime |> ctimeRenderPipeline.build().invoke
    /// 修改时间
    member self.mtime =
        record.mtime |> mtimeRenderPipeline.build().invoke
    /// 访问时间
    member self.atime =
        meta.atime |> atimeRenderPipeline.build().invoke

    /// 访问计数
    member self.view =
        meta.view |> viewRenderPipeline.build().invoke
    /// 星星计数
    member self.star =
        meta.star |> starRenderPipeline.build().invoke
