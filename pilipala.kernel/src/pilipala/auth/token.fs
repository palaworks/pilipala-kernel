module pilipala.auth.token

open System
open MySql.Data.MySqlClient
open fsharper.op
open fsharper.enhType
open fsharper.moreType
open pilipala
open pilipala.util.hash
open pilipala.util.uuid

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
    schema.tables
    >>= fun ts ->

            let table = ts.token

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

            schema.Managed().execute (sql, para)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok uuid
                    | _ -> Err FailedToCreateToken
            |> Some
    |> unwarp

/// 抹除凭据
let erase (token: string) =
    schema.tables
    >>= fun ts ->
            let table = ts.token
            let tokenHash = token.sha1

            schema.Managed().executeDelete table ("tokenHash", tokenHash)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok()
                    | _ -> Err FailedToEraseToken
            |> Some
    |> unwarp

/// 检查token是否合法
let check (token: string) =
    schema.tables
    >>= fun ts ->
            let table = ts.token

            let sql =
                $"SELECT COUNT(*) FROM {table} WHERE tokenHash=?tokenHash"

            let tokenHash = token.sha1

            let para =
                [| MySqlParameter("tokenHash", tokenHash) |]

            schema.Managed().getFstVal (sql, para)
            >>= fun n ->
                    //如果查询到的凭据记录唯一
                    match n with
                    | Some x when x = 0 -> Ok false
                    | Some x when cast x = 1 ->
                        //更新凭据访问记录
                        (table, ("atime", DateTime.Now), ("tokenHash", tokenHash))
                        |> schema.Managed().executeUpdate
                        >>= fun f ->
                                match f <| eq 1 with
                                | 1 -> Ok true
                                | __ -> Err FailedToUpdateTokenAtime
                    | _ -> Err DuplicateToken
            |> Some

    |> unwarp
