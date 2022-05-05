[<AutoOpen>]
module pilipala.builder.usePostCache

type palaBuilder with

    /// 启用文章缓存
    /// 启用该选项会影响文章的时实性，但能显著提升文章加载速度
    member self.usePostCache() = ()
