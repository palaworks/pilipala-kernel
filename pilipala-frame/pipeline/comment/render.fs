namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op.Alias
open pilipala.pipeline

type ICommentRenderPipelineBuilder =
    abstract Body: BuilderItem<u64, u64 * string>
    abstract CreateTime: BuilderItem<u64, u64 * DateTime>
    abstract Item: string -> BuilderItem<u64, u64 * obj>

    //用于遍历Item
    inherit IEnumerable<KeyValuePair<string, BuilderItem<u64, u64 * obj>>>
