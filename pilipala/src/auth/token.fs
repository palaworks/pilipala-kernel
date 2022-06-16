namespace pilipala.auth.token

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open DbManaged
open DbManaged.PgSql
open DbManaged.PgSql.ext.String
open pilipala
open pilipala.db
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

[<AutoOpen>]
module fn =
    /// 创建凭据
    /// 返回凭据值
    let create () =
        let table = tables.token

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

        let aff =
            mkCmd()
                .query(sql, paras)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 2 then
            Ok uuid
        else
            Err FailedToCreateToken

    /// 抹除凭据
    let erase (token: string) =
        let table = tables.token
        let tokenHash = token.sha1

        let aff =
            mkCmd()
                .delete(table, "tokenHash", tokenHash)
                .whenEq(1)
                .executeQuery ()

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToEraseToken

    /// 检查token是否合法
    let check (token: string) =
        let table = tables.token

        let sql =
            $"SELECT COUNT(*) FROM {table} WHERE tokenHash = <tokenHash>"
            |> normalizeSql

        let tokenHash = token.sha1

        let paras: (string * obj) list = [ ("tokenHash", tokenHash) ]

        let n =
            mkCmd().getFstVal(sql, paras).executeQuery ()

        match n with
        | Some x when x = 0 -> false
        //如果查询到的凭据记录唯一
        | Some x when coerce x = 1 ->
            //更新凭据访问记录
            let aff =
                mkCmd()
                    .update(table, ("atime", DateTime.Now), ("tokenHash", tokenHash))
                    .whenEq(1)
                    .executeQuery ()

            if aff |> eq 1 then
                true
            else
                raise FailedToUpdateTokenAtime
        | _ -> raise DuplicateToken
