namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open pilipala.pipeline

type ICommentModifyPipelineBuilder =
    abstract Body: BuilderItem<u64 * string>
    abstract CreateTime: BuilderItem<u64 * DateTime>
