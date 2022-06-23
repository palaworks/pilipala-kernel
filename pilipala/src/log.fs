namespace pilipala.log

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Generic
open System.Text.RegularExpressions
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open fsharper.typ.Pipe.Pipable
open fsharper.typ.List
open fsharper.op.Assert
open pilipala.util.uuid
open pilipala.util.stream
open pilipala.util.encoding

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
