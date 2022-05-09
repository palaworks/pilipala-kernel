module internal pilipala.container.cache

open System.Collections.Concurrent
open fsharper.op
open fsharper.op.Alias
open fsharper.typ
open fsharper.typ.Ord
open DbManaged.PgSql.ext.String
open pilipala
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
    let exist, (weight, value) = cache.TryGetValue((id, key))

    if exist then
        cache.[(id, key)] <- (palaflake.gen (), value) //更新权重
        value |> coerce |> Some
    else
        None

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
        match get typeId key with
        | Some v -> v
        | None ->
            let sql =
                $"SELECT {key} FROM {table} WHERE {typeName} = <{typeName}>"
                |> normalizeSql

            let paras: (string * obj) list = [ (typeName, typeId) ]

            db.Managed().getFstVal (sql, paras)
            >>= fun r ->
                    let value = r.unwrap ()

                    //写入缓存并返回
                    set typeId key value
                    value |> Ok
            |> unwrap
        |> coerce

    ///写缓存
    member self.set key value =
        set typeId key value //先写入缓存

        fun _ ->
            (table, (key, value), (typeName, typeId))
            |> db.Managed().executeUpdate
            >>= fun f ->

                    //当更改记录数为 1 时才会提交事务并追加到缓存头
                    match f <| eq 1 with
                    | 1 -> Ok()
                    | _ ->
                        rm typeId key //撤回写入（这里作简单化处理，直接清除缓存）
                        Err FailedToSyncDbException
        |> queueTask
