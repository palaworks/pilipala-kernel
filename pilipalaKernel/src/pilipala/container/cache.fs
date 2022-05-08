module internal pilipala.container.cache

open System.Collections.Concurrent
open fsharper.op
open fsharper.op.Alias
open fsharper.typ
open pilipala.util


//let cacheSize = 1024


//使用基于时间的缓存清理策略
//老旧的缓存在超出容量限制后将被清除

let private cache =
    //palaflake能够保证各池中id的唯一性
    //key构成： id * 字段名
    //val构成：权重 * 缓存值
    ConcurrentDictionary<u64 * string, u64 * obj>()

/// 取缓存数据
let get id key =
    let exist, (weight, value) = cache.TryGetValue((id, key))

    if exist then
        cache.[(id, key)] <- (palaflake.gen (), value) //更新权重
        value |> coerce |> Some
    else
        None

/// 写缓存数据
let set id key value =
    cache.AddOrUpdate((id, key), (palaflake.gen (), value), (fun key _ -> (palaflake.gen (), value)))
    |> ignore

/// 清除缓存数据
let rm id key = cache.TryRemove((id, key)) |> ignore
