module internal pilipala.container.cache

open System.Collections.Generic
open fsharper.op
open fsharper.types
open pilipala.util


//let cacheSize = 1024

//TODO 需考虑线程安全

//使用基于时间的缓存清理策略
//老旧的缓存在超出容量限制后将被清除

let private cache =
    Dictionary<string * uint64 * string, uint64 * obj>()

/// 取字段值
let inline get pool id key =
    if cache.ContainsKey(pool, id, key) then
        let value = cache.[(pool, id, key)] |> snd

        cache.[(pool, id, key)] <- (palaflake.gen (), value) //更新权重

        value |> coerce |> Some
    else
        None

/// 写字段值
let inline set pool id key value =
    if cache.ContainsKey(pool, id, key) then
        cache.[(pool, id, key)] <- (palaflake.gen (), value)
    else
        cache.Add((pool, id, key), (palaflake.gen (), value))
