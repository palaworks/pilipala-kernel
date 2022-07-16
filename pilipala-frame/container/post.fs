namespace pilipala.container.post

open System

type IPost =
    abstract member Body: string with get, set
    abstract member CreateTime: DateTime with get, set
    abstract member AccessTime: DateTime with get, set
    abstract member ModifyTime: DateTime with get, set
