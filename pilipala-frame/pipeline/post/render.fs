namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open pilipala.pipeline

type IPostRenderPipelineBuilder =
    abstract member cover: BuilderItem<u64, u64 * string>
    abstract member title: BuilderItem<u64, u64 * string>
    abstract member summary: BuilderItem<u64, u64 * string>
    abstract member body: BuilderItem<u64, u64 * string>
    abstract member ctime: BuilderItem<u64, u64 * DateTime>
    abstract member mtime: BuilderItem<u64, u64 * DateTime>
    abstract member atime: BuilderItem<u64, u64 * DateTime>
    abstract member view: BuilderItem<u64, u64 * u32>
    abstract member star: BuilderItem<u64, u64 * u32>
