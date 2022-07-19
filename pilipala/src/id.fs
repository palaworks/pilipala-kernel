namespace pilipala.id

open pilipala.util.id

type PalaflakeGenerator(machineId) =
    let g =
        palaflake.Generator(machineId, 2022us)

    interface IPalaflakeGenerator with
        member self.next() = g.Next()

type UuidGenerator() =
    let g = uuid.Generator(N)

    interface IUuidGenerator with
        member self.next() = g.next ()
