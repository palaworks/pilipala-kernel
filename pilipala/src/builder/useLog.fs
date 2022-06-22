[<AutoOpen>]
module pilipala.builder.useLog

open Microsoft.Extensions.Logging
open fsharper.typ.Pipe.Pipable
open pilipala.log

type Builder with

    member self.useLog t =

        let func _ = regLogByType t

        self.buildPipeline.mappend (Pipe(func = func))

    member self.useLog<'s when 's :> ILogger>() = self.useLog typeof<'s>
