namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.container.post

type IPostInitPipelineBuilder =
    abstract Batch: BuilderItem<IPost, u64 * IPost>
