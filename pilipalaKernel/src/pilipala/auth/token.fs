module pilipala.auth.token

open System
open fsharper.op
open fsharper.types
open fsharper.types.Ord
open pilipala
open pilipala.util.hash
open pilipala.util.uuid
open DbManaged.PgSql.ext.String

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
    db.tables
    >>= fun ts ->

            let table = ts.token

            let sql =
                $"INSERT INTO {table} \
                    ( tokenHash,  ctime,  atime) \
                    VALUES \
                    (<tokenHash>,<ctime>,<atime>)"
                |> normalizeSql

            let uuid = gen N

            let paras: (string * obj) list =
                [ ("tokenHash", uuid.sha1)
                  ("ctime", DateTime.Now)
                  ("atime", DateTime.Now) ]

            db.Managed().executeAny (sql, paras)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok uuid
                    | _ -> Err FailedToCreateToken
            |> Some
    |> unwrap

/// 抹除凭据
let erase (token: string) =
    db.tables
    >>= fun ts ->
            let table = ts.token
            let tokenHash = token.sha1

            db.Managed().executeDelete table ("tokenHash", tokenHash)
            >>= fun f ->
                    match f <| eq 1 with
                    | 1 -> Ok()
                    | _ -> Err FailedToEraseToken
            |> Some
    |> unwrap

/// 检查token是否合法
let check (token: string) =
    db.tables
    >>= fun ts ->
            let table = ts.token

            let sql =
                $"SELECT COUNT(*) FROM {table} WHERE tokenHash = <tokenHash>"
                |> normalizeSql

            let tokenHash = token.sha1

            let paras: (string * obj) list = [ ("tokenHash", tokenHash) ]

            db.Managed().getFstVal (sql, paras)
            >>= fun n ->
                    //如果查询到的凭据记录唯一
                    match n with
                    | Some x when x = 0 -> Ok false
                    | Some x when coerce x = 1 ->
                        //更新凭据访问记录
                        (table, ("atime", DateTime.Now), ("tokenHash", tokenHash))
                        |> db.Managed().executeUpdate
                        >>= fun f ->
                                match f <| eq 1 with
                                | 1 -> Ok true
                                | __ -> Err FailedToUpdateTokenAtime
                    | _ -> Err DuplicateToken
            |> Some

    |> unwrap
