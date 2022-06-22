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
    let registeredLogInfo = List<Type * LogLevel>()

    /// 注册日志
    let regLogByType t lv = registeredLogInfo.Add(t, lv)
    
    /// 注册日志
    let regLog<'l when 'l :> ILogger> lv = regLogByType typeof<'l> lv
