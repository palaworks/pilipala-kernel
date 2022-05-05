[<AutoOpen>]
module pilipala.builder.useCommentCache

type palaBuilder with

    /// 启用评论缓存
    /// 启用该选项会影响评论的时实性，但能显著提升文章加载速度
    member self.usePostCache() = ()

