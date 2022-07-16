namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open pilipala.pipeline

type ICommentRenderPipelineBuilder =
    abstract Body: BuilderItem<u64, u64 * string>
(*
    abstract member nick: BuilderItem<u64, u64 * string>
    abstract member content: BuilderItem<u64, u64 * string>
    abstract member email: BuilderItem<u64, u64 * string>
    abstract member site: BuilderItem<u64, u64 * string>
    abstract member ctime: BuilderItem<u64, u64 * DateTime>
*)
