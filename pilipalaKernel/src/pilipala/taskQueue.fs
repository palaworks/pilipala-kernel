module pilipala.taskQueue

open System.Threading.Tasks
open System.Collections.Concurrent
open fsharper.typ.Result'

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
