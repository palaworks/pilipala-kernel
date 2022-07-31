namespace pilipala.pipeline.user

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.access.user

type IUserFinalizePipelineBuilder =
    abstract Batch: BuilderItem<u64, u64 * IUser>
