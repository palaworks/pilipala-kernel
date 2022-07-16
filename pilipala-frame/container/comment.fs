namespace pilipala.container.comment

open System

type IComment =
    abstract member Body: string with get, set
