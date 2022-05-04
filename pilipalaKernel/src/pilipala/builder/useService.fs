[<AutoOpen>]
module pilipala.builder.useService

open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.service

type palaBuilder with

    /// 使用服务，采用默认日志流
    member self.useService<'s>() =
        let func _ = regService<'s> self.genLogStream

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self

    /// 使用带日志流的服务
    member self.useService<'s>(logStreamGetter) =
        let func _ = regService<'s> logStreamGetter

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
