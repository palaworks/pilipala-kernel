[<AutoOpen>]
module pilipala.builder.useService

open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.log
open pilipala.serv

type palaBuilder with

    /// 使用服务，采用默认日志流
    member self.useService<'s>() =
        let func _ = regService<'s> genLogStream

        self.buildPipeline.mappend(Pipe(func = func))

    /// 使用带日志流的服务
    member self.useService<'s>(logStreamGetter) =
        let func _ = regService<'s> logStreamGetter

        self.buildPipeline.mappend(Pipe(func = func))
