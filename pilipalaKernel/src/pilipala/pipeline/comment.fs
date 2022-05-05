module pilipala.pipeline.comment

open System
open fsharper.typ.Pipe.Pipable

let mutable commentIdRenderPipeline = Pipe<uint64>()
let mutable ownerMetaIdRenderPipeline = Pipe<uint64>()
let mutable replyToRenderPipeline = Pipe<uint64>()
let mutable nickRenderPipeline = Pipe<string>()
let mutable contentRenderPipeline = Pipe<string>()
let mutable emailRenderPipeline = Pipe<string>()
let mutable siteRenderPipeline = Pipe<string>()
let mutable ctimeRenderPipeline = Pipe<DateTime>()
