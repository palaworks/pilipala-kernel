namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.container.comment

type ICommentInitPipelineBuilder =
    abstract Batch: BuilderItem<IComment, u64 * IComment>
