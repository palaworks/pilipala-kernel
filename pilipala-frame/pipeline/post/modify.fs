namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.pipeline

type IPostModifyPipelineBuilder =
    abstract Title: BuilderItem<u64 * string>
    abstract Body: BuilderItem<u64 * string>
    abstract CreateTime: BuilderItem<u64 * DateTime>
    abstract AccessTime: BuilderItem<u64 * DateTime>
    abstract ModifyTime: BuilderItem<u64 * DateTime>
