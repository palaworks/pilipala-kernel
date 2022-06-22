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
    /// 已注册日志集合
    let private registeredLog = ServiceCollection()

    /// 使用类型注册日志
    let regLogByType t = registeredLog.AddTransient t |> ignore
    /// 注册日志
    let regLog<'l when 'l :> ILogger> () = regLogByType typeof<'l>
