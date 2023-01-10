namespace pilipala.id

open pilipala.util.id

module IUuidGenerator =

    let make () =
        let g = uuid.Generator(N) //全局应始终使用N型ID生成

        { new IUuidGenerator with
            member self.next() = g.next () }
