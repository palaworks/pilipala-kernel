[<AutoOpen>]
module pilipala.builder.useLog

open fsharper.typ
open Microsoft.Extensions.DependencyInjection
open pilipala.log

type Builder with

    member self.useLogProvider provider =

        let f (sc: IServiceCollection) =
            sc
                .BuildServiceProvider()
                .GetService<LogProvider>()
                .regLogProvider provider

            sc

        { pipeline = self.pipeline .> f }

    member self.useLogFilter category lv =

        let f (sc: IServiceCollection) =
            sc.BuildServiceProvider().GetService<LogProvider>()
                .regLogFilter
                category
                lv

            sc

        { pipeline = self.pipeline .> f }
