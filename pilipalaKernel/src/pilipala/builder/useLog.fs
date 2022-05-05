[<AutoOpen>]
module pilipala.builder.useLog

open System
open System.IO
open System.Reflection
open System.Threading.Tasks
open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.service
open pilipala.log
open pilipala.util.stream

type palaBuilder with

    /// 使用日志
    /// 多次调用会将流依次组合
    member self.useLog logStreamGetter =

        let func _ =
            self.logStreamGetterList <- logStreamGetter :: self.logStreamGetterList
            let arr = self.logStreamGetterList.toArray ()

            //使空流的判断开销转移至构建期
            genLogStream <- fun () -> new StreamDistributor([| for f in arr -> f () |])

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
