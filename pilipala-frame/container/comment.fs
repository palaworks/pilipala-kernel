namespace pilipala.container.comment

open System

type IComment =
    //逻辑字段
    //abstract UserName: string with get, set
    abstract Body: string with get, set
    abstract CreateTime: DateTime with get, set
//逻辑字段
//abstract Replies: IComment list with get, set
