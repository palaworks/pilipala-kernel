namespace pilipala.container.post

open pilipala.pipeline.post

//值容器
//对该容器的更改不会反映到持久化层次
type Post private (meta: PostMeta, record: PostRecord) =
    //惰性求值

    let meta = meta
    let record = record

    new(postId: uint64) =
        let meta = PostMeta(postId)
        let record = PostRecord(meta.currRecordId)
        Post(meta, record)

    /// Id
    member self.Id() = meta.metaId
    /// 封面
    member self.Cover() =
        record.cover |> coverRenderPipeline.build().invoke
    /// 标题
    member self.Title() =
        record.title |> titleRenderPipeline.build().invoke
    /// 概要
    member self.Summary() =
        record.summary
        |> summaryRenderPipeline.build().invoke
    /// 正文
    member self.Body() =
        record.body |> bodyRenderPipeline.build().invoke
    /// 创建时间
    member self.Ctime() =
        meta.ctime |> ctimeRenderPipeline.build().invoke
    /// 修改时间
    member self.Mtime() =
        record.mtime |> mtimeRenderPipeline.build().invoke
    /// 访问时间
    member self.Atime() =
        meta.atime |> atimeRenderPipeline.build().invoke
    /// 访问计数
    member self.View() =
        meta.view |> viewRenderPipeline.build().invoke
    /// 星星计数
    member self.Star() =
        meta.star |> starRenderPipeline.build().invoke
