namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open pilipala.pipeline

type ICommentRenderPipelineBuilder =
    //abstract UserName: BuilderItem<u64, u64 * string>
    abstract Body: BuilderItem<u64, u64 * string>
    abstract CreateTime: BuilderItem<u64, u64 * DateTime>
