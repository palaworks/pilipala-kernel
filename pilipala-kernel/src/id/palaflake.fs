namespace pilipala.id

open fsharper.alias
open pilipala.util.id

module IPalaflakeGenerator =

    let make serverId =
        let g =
            palaflake.Generator(serverId, 2022us)

        { new IPalaflakeGenerator with
            member self.next() = u64 (g.Next()) }
