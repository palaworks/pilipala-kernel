namespace pilipala.container.post

open System
open fsharper.op.Alias

type IPost =
    abstract Id: u64
    abstract Title: string with get, set
    abstract Body: string with get, set
    abstract CreateTime: DateTime with get, set
    abstract AccessTime: DateTime with get, set
    abstract ModifyTime: DateTime with get, set
