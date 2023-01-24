[<AutoOpen>]
module pilipala.builder.useLog

open Microsoft.Extensions.DependencyInjection
open fsharper.typ
open pilipala.log
open pilipala.util.di

type Builder with

    member self.useLoggerProvider provider =

        let f (sc: IServiceCollection) =
            sc.UpdateSingleton<LoggerRegister>
            <| fun old -> old.registerLoggerProvider provider

        { pipeline = self.pipeline .> effect f }

    member self.useLoggerFilter category lv =

        let f (sc: IServiceCollection) =
            sc.UpdateSingleton<LoggerRegister>
            <| fun old -> old.registerLoggerFilter (category, lv)

        { pipeline = self.pipeline .> effect f }
