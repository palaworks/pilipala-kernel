namespace pilipala.container.post

module stack =

    open System
    open MySql.Data.MySqlClient
    open pilipala.data
    open pilipala.util
    open pilipala.kernel
    open pilipala.container
    open pilipala.container.cache


    type PostStack(stackId: uint64) =
        //文章栈是一种单向栈（只进不出）

        let fromCache key = KVPool.get "stack" stackId key
        let intoCache key value = KVPool.set "stack" stackId key value

        /// 取字段值
        member inline private this.get key =
            (fromCache key).unwarpOr
            <| fun _ ->
                unwarp (
                    pala ()
                    >>= fun p ->
                            let table = p.table.stack

                            let sql =
                                $"SELECT {key} FROM {table} WHERE stackId = ?stackId"

                            let para = [| MySqlParameter("stackId", stackId) |]

                            p.database.getFstVal (sql, para)
                            >>= fun r ->
                                    let value = r.unwarp ()

                                    intoCache key value //写入缓存并返回
                                    value |> cast |> Ok
                )

        /// 写字段值
        member inline private this.set key value =
            pala ()
            >>= fun p ->
                    let table = p.table.stack

                    (table, (key, value), ("stackId", stackId))
                    |> p.database.executeUpdate
                    >>= fun f ->

                            //当更改记录数为 1 时才会提交事务并追加到缓存头
                            match f <| eq 1 with
                            | 1 -> Ok <| intoCache key value
                            | _ -> Err FailedToWriteCache


        /// 栈id
        member this.stackId = stackId
        /// 上级栈id
        member this.superStackId
            with get (): uint64 = this.get "superStackId"
            and set (v: uint64) = (this.set "superStackId" v).unwarp ()
        /// 当前记录id
        member this.currRecordId
            with get (): uint64 = this.get "currRecordId"
            and set (v: uint64) = (this.set "currRecordId" v).unwarp ()
        /// 创建时间
        member this.ctime
            with get (): DateTime = this.get "ctime"
            and set (v: DateTime) = (this.set "ctime" v).unwarp ()
        /// 访问时间
        member this.atime
            with get (): DateTime = this.get "atime"
            and set (v: DateTime) = (this.set "atime" v).unwarp ()
        /// 访问数
        member this.view
            with get (): unit = this.get "view"
            and set (v: unit) = (this.set "view" v).unwarp ()
        /// 星星数
        member this.star
            with get (): unit = this.get "star"
            and set (v: unit) = (this.set "star" v).unwarp ()



    /// 创建文章栈
    /// 返回文章栈id
    let create () =
        pala ()
        >>= fun p ->
                let table = p.table.stack

                let sql =
                    $"INSERT INTO {table} \
                    ( stackId, superStackId, currRecordId, ctime, atime, view, star) \
                    VALUES \
                    (?stackId,?superStackId,?currRecordId,?ctime,?atime,?view,?star)"

                let stackId = palaflake.gen ()

                let recordId = 0 //初始栈空

                let para =
                    [| MySqlParameter("stackId", stackId)
                       MySqlParameter("superStackId", 0)
                       MySqlParameter("currRecordId", recordId)
                       MySqlParameter("ctime", DateTime.Now)
                       MySqlParameter("atime", DateTime.Now)
                       MySqlParameter("view", 0)
                       MySqlParameter("star", 0) |]

                p.database.execute (sql, para)
                >>= fun f ->
                        
                        match f <| eq 1 with
                        | 1 -> Ok stackId
                        | _ -> Err FailedToCreateStack

    /// 抹除文章栈
    let erase (stackId: uint64) =
        pala ()
        >>= fun p ->
                let table = p.table.stack

                p.database.executeDelete table ("stackId", stackId)
                >>= fun f ->
                        
                        match f <| eq 1 with
                        | 1 -> Ok()
                        | _ -> Err FailedToEraseStack

(*
    type PostStack with

        /// 得到最大栈id
        member _.maxStackId =
            pala()
            >>= fun p ->
                    $"SELECT MAX(stackId) FROM {p.table.stack}"
                    |> p.database.getFstVal
                    >>= fun value ->
                            match value with
                            | :? uint -> Some value
                            | _ -> None //DBNULL及其他情况为None

        /// 得到最小栈id
        member _.minStackId =
            pala()
            >>= fun p ->
                    $"SELECT MIN(stackId) FROM {p.table.stack}"
                    |> p.database.getFstVal
                    >>= fun value ->
                            match value with
                            | :? uint -> Some value
                            | _ -> None //DBNULL及其他情况为None
*)
