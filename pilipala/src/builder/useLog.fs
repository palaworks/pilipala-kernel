[<AutoOpen>]
module pilipala.builder.useLog

open fsharper.typ.Pipe.Pipable
open Microsoft.Extensions.Logging
open pilipala.log

type Builder with

    member self.useLog(t, lv: LogLevel) =

        let func _ = regLogByType t lv

        self.buildPipeline.mappend (Pipe(func = func))

    member self.useLog t = self.useLog (t, LogLevel.Information)

    member self.useLog<'l when 'l :> ILogger and 'l: not struct>() = self.useLog typeof<'l>

    member self.useLog<'l when 'l :> ILogger and 'l: not struct> lv = self.useLog (typeof<'l>, lv)

//日志类型不应允许从程序集注册，因为它不应具有自主性功能
(*
    /// 从程序集文件夹注册
    member self.useLog dir =
        let logDir = DirectoryInfo(dir)
        let logName = logDir.Name

        let logDll =
            logDir.GetFileSystemInfos().toList ()
            |> filterOne (fun x -> x.Name = $"{logName}.dll")
            |> unwrap

        let logDllPath = logDll.FullName

        let logType =
            Assembly
                .LoadFrom(logDllPath)
                .GetType($"pilipala.log.{logName}")

        self.useLog logType
*)
