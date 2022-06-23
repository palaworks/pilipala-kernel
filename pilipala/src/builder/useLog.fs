[<AutoOpen>]
module pilipala.builder.useLog

open fsharper.typ.Pipe.Pipable
open Microsoft.Extensions.Logging
open pilipala.log

type Builder with

    member self.useLogProvider provider =

        let func _ = regLogProvider provider 

        self.buildPipeline.mappend (Pipe(func = func))

    member self.useLogFilter category lv =
        
        let func _ = regLogFilter category lv

        self.buildPipeline.mappend (Pipe(func = func))
