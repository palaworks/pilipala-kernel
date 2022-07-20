namespace pilipala.id

open pilipala.util.id

type UuidGenerator() =
    let g = uuid.Generator(N)

    interface IUuidGenerator with
        member self.next() = g.next ()
