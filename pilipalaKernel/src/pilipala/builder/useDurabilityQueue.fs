[<AutoOpen>]
module pilipala.builder.useDurabilityQueue

open fsharper.typ.Pipe.Pipable
open pilipala

type palaBuilder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDurabilityQueue() =
        let func _ =
            self.useDurabilityQueue <- true //设置启用持久化队列标记
            taskQueue.queueTask <- fun f -> f () |> ignore

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
