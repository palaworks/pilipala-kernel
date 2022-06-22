[<AutoOpen>]
module pilipala.builder.useService

open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.log
open pilipala.serv.reg
open pilipala.serv

type Builder with

    /// 使用服务
    member self.useServ<'s when 's :> ServAttribute and 's: not struct>() =
        let func _ = regServ<'s> ()

        self.buildPipeline.mappend (Pipe(func = func))
