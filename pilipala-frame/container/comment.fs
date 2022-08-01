namespace pilipala.container.comment

open System
open fsharper.typ
open fsharper.op.Alias

type CommentBinding =
    | BindPost of u64
    | BindComment of u64

type IComment =
    abstract Id: u64
    abstract Body: string with get, set
    abstract CreateTime: DateTime with get, set
    abstract Binding: CommentBinding with get, set
    abstract Item: string -> Option'<obj> with get, set

type ICommentProvider =
    abstract fetch: u64 -> IComment
    abstract create: IComment -> u64
    abstract delete: u64 -> u64 * IComment
