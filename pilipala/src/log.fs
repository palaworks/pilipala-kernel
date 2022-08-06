namespace pilipala.log

open System.Collections.Generic
open fsharper.alias
open Microsoft.Extensions.Logging

type internal LogRegister() =
    
    /// 已注册日志信息
    member self.registeredLoggerProvider = List<ILoggerProvider>()

    /// 已注册日志过滤器
    member self.registeredLoggerFilter = Dict<string, LogLevel>()

    /// 注册日志提供者
    member self.regLoggerProvider provider = self.registeredLoggerProvider.Add provider

    /// 注册日志过滤器
    member self.regLogFilter category lv =
        self.registeredLoggerFilter.Add(category, lv)
