namespace pilipala.log

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Generic
open System.Text.RegularExpressions
open fsharper.typ.Pipe.Pipable
open fsharper.typ.List
open fsharper.op.Assert
open pilipala.util.uuid
open pilipala.util.stream
open pilipala.util.encoding

(*
logPath是用于路由日志结构的文本
例如：
/log/fs
/log/mem
/log/some_plugin/1a2b...
*)
type logPath = string

[<AutoOpen>]
module typ =

    type ILog =
        inherit IDisposable

        abstract member log : string -> unit
        abstract member logLine : string -> unit
        abstract member logAsync : string -> Task
        abstract member logLineAsync : string -> Task

(*
    type Log(s: Stream) =
        let sw = new StreamWriter(s)
        do sw.AutoFlush <- true

        member self.log(text: string) = sw.Write text
        member self.logLine(text: string) = sw.WriteLine text
        member self.logAsync(text: string) = sw.WriteAsync text
        member self.logLineAsync(text: string) = sw.WriteLineAsync text

        member self.Dispose() = sw.Dispose()
*)


[<AutoOpen>]
module fn =
    /// 已注册日志构造器集合
    let private registeredLogCons = Dictionary<logPath, unit -> ILog>()

    let regLog logPath logCons = registeredLogCons.Add(logPath, logCons)

    let getLog logPath =
        let ok, logCons = registeredLogCons.TryGetValue(logPath)
        ok |> mustTrue
        logCons ()

    let matchLog logPathRegexp =
        [ for logPath in registeredLogCons.Keys do
              if Regex.IsMatch(logPath, logPathRegexp) then
                  let ok, cons = registeredLogCons.TryGetValue(logPath)
                  ok |> mustTrue
                  cons () ]
