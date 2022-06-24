namespace pilipala.log

open System.Collections.Generic
open Microsoft.Extensions.Logging

[<AutoOpen>]
module fn =

    /// 已注册日志信息
    let internal registeredLogProvider = List<ILoggerProvider>()

    /// 已注册日志过滤器
    let internal registeredLogFilter = Dictionary<string, LogLevel>()

    /// 注册日志提供者
    let regLogProvider provider = registeredLogProvider.Add provider

    /// 注册日志过滤器
    let regLogFilter category lv = registeredLogFilter.Add(category, lv)
