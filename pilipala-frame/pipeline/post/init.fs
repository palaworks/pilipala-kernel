namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.container.post
open pilipala.pipeline

type IPostInitPipelineBuilder =
    abstract Batch: BuilderItem<IPost, u64 * IPost>
