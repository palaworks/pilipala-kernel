namespace pilipala.id

open pilipala.util.palaflake

[<AutoOpen>]
module fn =
    /// 全局palaflake生成器
    let palaflake = Generator(0uy, 2022us)
