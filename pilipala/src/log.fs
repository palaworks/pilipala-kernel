namespace pilipala.log

open System.Collections.Generic
open Microsoft.Extensions.Logging

type internal LogProvider() =
    
    /// 已注册日志信息
    member self.registeredLoggerProvider = List<ILoggerProvider>()

    /// 已注册日志过滤器
    member self.registeredLoggerFilter = Dictionary<string, LogLevel>()

    /// 注册日志提供者
    member self.regLogProvider provider = self.registeredLoggerProvider.Add provider

    /// 注册日志过滤器
    member self.regLogFilter category lv =
        self.registeredLoggerFilter.Add(category, lv)
