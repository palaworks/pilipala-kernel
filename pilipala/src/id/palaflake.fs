namespace pilipala.id

open pilipala.util.id
open fsharper.op.Alias

module IPalaflakeGenerator =

    let make serverId =
        let g =
            palaflake.Generator(serverId, 2022us)

        { new IPalaflakeGenerator with
            member self.next() = u64 (g.Next()) }
