namespace pilipala.id

open fsharper.op.Alias

type IPalaflakeGenerator =
    abstract member next: unit -> u64
