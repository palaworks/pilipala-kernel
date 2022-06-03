module pilipala.taskQueue

open System.Threading.Tasks
open System.Collections.Concurrent
open fsharper.typ.Result'

//TODO
//此处的任务队列用于实现pilipala总体的负载均衡
//持久化队列用于实现数据库访问的负载均衡

let private taskQueue =
    ConcurrentQueue<unit -> Result'<unit, exn>>()

let mutable queueTask = taskQueue.Enqueue //TODO 若不启用持久化队列，可从此处拦截

let forceLeftQueuedTask () =
    queueTask <- fun f -> f () |> ignore //拦截请求

    let rec loop () =
        match taskQueue.TryDequeue() with
        | true, f ->
            f () |> ignore //TODO need exn handler
            loop ()
        | _ -> ()

    loop ()

    queueTask <- taskQueue.Enqueue //恢复
