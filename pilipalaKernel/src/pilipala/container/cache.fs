module internal pilipala.container.cache

open System.Collections.Concurrent
open fsharper.op
open fsharper.op.Alias
open fsharper.typ
open fsharper.typ.Ord
open DbManaged
open DbManaged.PgSql
open DbManaged.PgSql.ext.String
open pilipala
open pilipala.db
open pilipala.util
open pilipala.taskQueue


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
        cache.[(id, key)] <- (palaflake.gen (), value) //更新权重
        value |> coerce |> Some
    | _ -> None

/// 写缓存
let set id key value =
    cache.AddOrUpdate((id, key), (palaflake.gen (), value), (fun key _ -> (palaflake.gen (), value)))
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

        fun _ ->
            let aff =
                mkCmd()
                    .update(table, (key, value), (typeName, typeId))
                    .whenEq(1)
                    .executeQuery ()
            //当更改记录数为 1 时才会提交事务并追加到缓存头
            if aff |> eq 1 then
                Ok()
            else
                rm typeId key //撤回写入（这里作简单化处理，直接清除缓存）
                Err FailedToSyncDbException
        |> queueTask
