namespace pilipala.id

open fsharper.op.Alias

type IUuidGenerator =
    abstract member next: unit -> string
