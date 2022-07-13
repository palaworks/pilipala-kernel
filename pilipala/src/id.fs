namespace pilipala.id

open pilipala.util
open pilipala.util.palaflake
open pilipala.util.uuid

type PalaflakeGenerator(machineId) =
    let g = Generator(machineId, 2022us)

    interface IPalaflakeGenerator with
        member self.next() = g.Next()

type UuidGenerator(format: UuidFormat) =

    interface IUuidGenerator with
        member self.next() = gen format
