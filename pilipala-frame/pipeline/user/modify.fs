namespace pilipala.pipeline.user

open System
open System.Collections.Generic
open fsharper.op.Alias
open pilipala.pipeline

type IUserModifyPipelineBuilder =
    abstract Name: BuilderItem<u64 * string>
    abstract Email: BuilderItem<u64 * string>
    abstract CreateTime: BuilderItem<u64 * DateTime>
    abstract Item: string -> BuilderItem<u64 * obj>

    //用于遍历Item
    inherit IEnumerable<KeyValuePair<string, BuilderItem<u64 * obj>>>
