namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.container.comment

type ICommentFinalizePipelineBuilder =
    abstract Batch: BuilderItem<u64, u64 * IComment>
