[<AutoOpen>]
module pilipala.builder.useService

open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.log
open pilipala.serv

type palaBuilder with

    /// 使用服务
    member self.useServ<'s when 's: not struct> path =
        let func _ = regServ<'s> path

        self.buildPipeline.mappend (Pipe(func = func))
