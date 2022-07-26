namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.container.post
open pilipala.pipeline

type IPostFinalizePipelineBuilder =
    abstract Batch: BuilderItem<u64, u64 * IPost>
