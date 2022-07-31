namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.container.post

type IPostFinalizePipelineBuilder =
    abstract Batch: BuilderItem<u64, u64 * IPost>
