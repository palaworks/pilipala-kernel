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
                .GetService<LogRegister>()
                .regLoggerProvider provider

            sc

        { pipeline = self.pipeline .> f }

    member self.useLogFilter category lv =

        let f (sc: IServiceCollection) =
            sc.BuildServiceProvider().GetService<LogRegister>()
                .regLogFilter
                category
                lv

            sc

        { pipeline = self.pipeline .> f }
