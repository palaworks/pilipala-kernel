namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.pipeline

type IPostRenderPipelineBuilder =
    abstract Title: BuilderItem<u64, u64 * string>
    abstract Body: BuilderItem<u64, u64 * string>
    abstract CreateTime: BuilderItem<u64, u64 * DateTime>
    abstract AccessTime: BuilderItem<u64, u64 * DateTime>
    abstract ModifyTime: BuilderItem<u64, u64 * DateTime>
(*
    abstract Summary: BuilderItem<u64, u64 * string>
    abstract member cover: BuilderItem<u64, u64 * string>
    abstract member view: BuilderItem<u64, u64 * u32>
    abstract member star: BuilderItem<u64, u64 * u32>
    *)
