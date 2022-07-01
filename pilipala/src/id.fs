namespace pilipala.id

open pilipala.util.palaflake

type IdProvider(machineId) =

    /// 获得palaflake生成器
    member self.palaflake = Generator(machineId, 2022us)
