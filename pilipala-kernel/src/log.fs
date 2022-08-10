namespace pilipala.log

open System.Collections.Generic
open Microsoft.Extensions.Logging.Configuration
open fsharper.alias
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
