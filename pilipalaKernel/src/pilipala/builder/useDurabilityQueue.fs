[<AutoOpen>]
module pilipala.builder.useDurabilityQueue

type palaBuilder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDurabilityQueue() = ()
