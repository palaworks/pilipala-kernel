namespace pilipala.data.kv

open fsharper.op
open fsharper.typ
open fsharper.alias

module IKvProvider =

    let make () =
        let map = ConcurrentDict<obj, obj>()

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
