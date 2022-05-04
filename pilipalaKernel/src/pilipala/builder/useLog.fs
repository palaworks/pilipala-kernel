[<AutoOpen>]
module pilipala.builder.useLog

open System
open System.IO
open System.Reflection
open System.Threading.Tasks
open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.service
open pilipala

type palaBuilder with

    /// 使用日志
    /// 多次调用会将流依次组合
    member self.useLog logStreamGetter =
        let func _ =
            self.logStreamGetterSet.Add logStreamGetter
            |> ignore

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
