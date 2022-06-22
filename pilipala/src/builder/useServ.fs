[<AutoOpen>]
module pilipala.builder.useServ

open fsharper.typ.Pipe.Pipable
open pilipala.serv.reg

type Builder with

    member self.useServ t =

        let func _ = regServByType t

        self.buildPipeline.mappend (Pipe(func = func))

    member self.useServ<'s when 's :> ServAttribute>() = self.useServ typeof<'s>
