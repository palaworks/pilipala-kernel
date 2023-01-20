namespace pilipala.log

open fsharper.op
open Microsoft.Extensions.Logging

type LoggerRegister =
    { LoggerProviders: ILoggerProvider list
      LoggerFilters: (string * LogLevel) list }

//最终整合时应使用foldr以保证顺序
type LoggerRegister with

    /// 注册日志提供者
    member self.registerLoggerProvider(provider: ILoggerProvider) =
        { self with LoggerProviders = provider :: self.LoggerProviders }

    /// 注册日志过滤器
    member self.registerLoggerFilter(category: string, lv: LogLevel) =
        { self with LoggerFilters = (category, lv) :: self.LoggerFilters }

    member self.configure(builder: ILoggingBuilder) =
        self.LoggerFilters.foldr
        <| fun (k, v) (builder: ILoggingBuilder) -> builder.AddFilter(k, v)
        <| builder
        |> ignore

        self.LoggerProviders.foldr
        <| fun p (builder: ILoggingBuilder) -> builder.AddProvider p
        <| builder
        |> ignore
