namespace pilipala.pipeline.user

open System
open fsharper.op.Alias
open pilipala.pipeline
open pilipala.access.user

type IUserInitPipelineBuilder =
    abstract Batch: BuilderItem<IUser, u64 * IUser>
