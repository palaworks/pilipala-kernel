[<AutoOpen>]
module pilipala.builder.useLog

open fsharper.typ.Pipe
open Microsoft.Extensions.DependencyInjection
open pilipala.log

type Builder with

    member self.useLogProvider provider =

        let func _ =
            self
                .DI
                .BuildServiceProvider()
                .GetService<LogProvider>()
                .regLogProvider provider

        self.buildPipeline.export (StatePipe(activate = func))

    member self.useLogFilter category lv =

        let func _ =
            self.DI.BuildServiceProvider().GetService<LogProvider>()
                .regLogFilter
                category
                lv

        self.buildPipeline.export (StatePipe(activate = func))
