namespace pilipala.data.kv

open System
open System.Collections.Concurrent
open Newtonsoft
open Microsoft.Extensions.DependencyInjection
open Newtonsoft.Json
open fsharper.typ
open fsharper.op
open fsharper.typ.Pipe
open fsharper.op.Alias

module IKvProvider =

    let mk () =
        let map = ConcurrentDictionary<obj, obj>()

        { new IKvProvider with

            member i.get key =
                (map.TryGetValue(key) |> Option'.fromOkComma)
                    .fmap coerce

            member i.set key value =
                map.AddOrUpdate(key, (fun _ -> value), (fun _ _ -> value :> obj))
                |> ignore

            member i.del key = map.TryRemove key |> ignore

            member i.clear() = map.Clear()

            member i.exists key = map.ContainsKey key }
