namespace pilipala.container.comment

open System
open fsharper.typ
open fsharper.op.Alias

type IComment =
    abstract Id: u64
    abstract Body: string with get, set
    abstract CreateTime: DateTime with get, set
    abstract Item: string -> Option'<obj> with get, set
