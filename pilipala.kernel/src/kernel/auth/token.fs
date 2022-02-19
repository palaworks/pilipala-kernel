module pilipala.kernel.auth.token

open System
open MySql.Data.MySqlClient
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open pilipala.util.hash
open pilipala.util.uuid
open pilipala.launcher

/// 无法创建凭据错误
exception FailedToCreateToken
/// 无法抹除凭据错误
exception FailedToEraseToken
/// 无法更新凭据访问时间错误
exception FailedToUpdateTokenAtime
/// 凭据重复
exception DuplicateToken

/// 创建凭据
/// 返回凭据值
let create () =
    pala ()
    >>= fun p ->

            let table = p.table.token

            let sql =
                $"INSERT INTO {table} \
                    ( tokenHash, ctime, atime) \
                    VALUES \
                    (?tokenHash,?ctime,?atime)"

            let uuid = gen N

            let para =
                [| MySqlParameter("tokenHash", uuid.sha1)
                   MySqlParameter("ctime", DateTime.Now)
                   MySqlParameter("atime", DateTime.Now) |]

            p.database.execute (sql, para)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok uuid
                    | _ -> Err FailedToCreateToken

/// 抹除凭据
let erase (token: string) =
    pala ()
    >>= fun p ->
            let table = p.table.token
            let tokenHash = token.sha1

            p.database.executeDelete table ("tokenHash", tokenHash)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok()
                    | _ -> Err FailedToEraseToken

/// 检查token是否合法
let check (token: string) =
    pala ()
    >>= fun p ->

            let table = p.table.token

            let sql =
                $"SELECT COUNT(*) FROM {table} WHERE tokenHash=?tokenHash"

            let tokenHash = token.sha1

            let para =
                [| MySqlParameter("tokenHash", tokenHash) |]

            p.database.getFstVal (sql, para)
            >>= fun n ->
                    //如果查询到的凭据记录唯一
                    match n with
                    | Some x when cast x = 1 ->
                        //更新凭据访问记录
                        (table, ("atime", DateTime.Now), ("tokenHash", tokenHash))
                        |> p.database.executeUpdate
                        >>= fun f ->
                                match f <| eq 1 with
                                | 1 -> Ok true
                                | __ -> Err FailedToUpdateTokenAtime
                    | _ -> Err DuplicateToken
