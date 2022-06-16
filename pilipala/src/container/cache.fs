module internal pilipala.container.cache

open System.Threading.Tasks
open System.Collections.Concurrent
open fsharper.op
open fsharper.op.Alias
open fsharper.op.Async
open fsharper.typ
open fsharper.typ.Ord
open DbManaged
open DbManaged.PgSql
open DbManaged.PgSql.ext.String
open pilipala.id
open pilipala.db
open pilipala.util


//let cacheSize = 1024


//使用基于时间的缓存清理策略
//老旧的缓存在超出容量限制后将被清除

let private cache =
    //palaflake能够保证各池中id的唯一性
    //key构成： id * 字段名
    //val构成：权重 * 缓存值
    ConcurrentDictionary<u64 * string, u64 * obj>()

/// 取缓存
let get id key =
    match cache.TryGetValue((id, key)) with
    | true, (weight, value) ->
        cache.[(id, key)] <- (palaflake.Next(), value) //更新权重
        value |> coerce |> Some
    | _ -> None

/// 写缓存
let set id key value =
    cache.AddOrUpdate((id, key), (palaflake.Next(), value), (fun key _ -> (palaflake.Next(), value)))
    |> ignore

/// 清缓存
let rm id key = cache.TryRemove((id, key)) |> ignore


type ContainerCacheHandler(table, typeName, typeId) =

    //table:被检索的数据库表
    //typeName:WHERE字句索引名
    //typeId:WHERE字句索引值

    ///取缓存
    member self.get key =
        get typeId key |> unwrapOr
        <| fun _ ->
            let sql =
                $"SELECT {key} FROM {table} WHERE {typeName} = <{typeName}>"
                |> normalizeSql

            let paras: (string * obj) list = [ (typeName, typeId) ]

            mkCmd().getFstVal(sql, paras).executeQuery ()
            >>= fun value ->
                    //写入缓存并返回
                    set typeId key value
                    Some value
            |> unwrap
        |> coerce

    ///写缓存
    member self.set key value =
        set typeId key value //先写入缓存

        mkCmd()
            .update(table, (key, value), (typeName, typeId))
            .whenEq(1)
            .queueQuery()
            .Then(fun t ->
                let aff = t |> result

                if aff |> eq 1 |> not then
                    //撤回写入（这里作简单化处理，直接清除缓存）
                    rm typeId key)
        |> ignore
