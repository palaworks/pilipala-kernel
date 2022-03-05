module pilipala.pipeline.post

open System
open fsharper.moreType.Pipable


let mutable coverRenderPipeline = Pipe<string>()
let mutable titleRenderPipeline = Pipe<string>()
let mutable summaryRenderPipeline = Pipe<string>()
let mutable bodyRenderPipeline = Pipe<string>()
let mutable ctimeRenderPipeline = Pipe<DateTime>()
let mutable mtimeRenderPipeline = Pipe<DateTime>()
let mutable atimeRenderPipeline = Pipe<DateTime>()
let mutable viewRenderPipeline = Pipe<uint32>()
let mutable starRenderPipeline = Pipe<uint32>()
