namespace pilipala.container.post

open System

type IPost =
    abstract Title: string with get, set
    abstract Body: string with get, set
    abstract CreateTime: DateTime with get, set
    abstract AccessTime: DateTime with get, set
    abstract ModifyTime: DateTime with get, set
//逻辑字段
//abstract Comments: IComment list with get, set
