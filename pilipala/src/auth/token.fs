namespace pilipala.auth.token

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open DbManaged
open DbManaged.PgSql
open pilipala
open pilipala.data.db
open pilipala.id
open pilipala.util.id
open pilipala.util.hash

/// 无法创建凭据错误
exception FailedToCreateToken
/// 无法抹除凭据错误
exception FailedToEraseToken
/// 无法更新凭据访问时间错误
exception FailedToUpdateTokenAtime
/// 凭据重复
exception DuplicateToken

type internal TokenProvider(dp: IDbProvider, uuid: IUuidGenerator) =

    /// 创建凭据
    /// 返回凭据值
    member self.create() =
        let table = dp.tables.token

        let sql =
            $"INSERT INTO {table} \
                    ( tokenHash,  ctime,  atime) \
                    VALUES \
                    (<tokenHash>,<ctime>,<atime>)"
            |> dp.managed.normalizeSql


        let paras: (string * obj) list =
            [ ("tokenHash", uuid.next().sha1)
              ("ctime", DateTime.Now)
              ("atime", DateTime.Now) ]

        let aff =
            dp.mkCmd().query(sql, paras).whenEq (1)
            |> dp.managed.executeQuery

        if aff |> eq 2 then
            Ok uuid
        else
            Err FailedToCreateToken

    /// 抹除凭据
    member self.erase(token: string) =
        let table = dp.tables.token
        let tokenHash = token.sha1

        let aff =
            dp
                .mkCmd()
                .delete(table, "tokenHash", tokenHash)
                .whenEq 1
            |> dp.managed.executeQuery

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToEraseToken

    /// 检查token是否合法
    member self.check(token: string) =
        let table = dp.tables.token

        let sql =
            $"SELECT COUNT(*) FROM {table} WHERE tokenHash = <tokenHash>"
            |> dp.managed.normalizeSql

        let tokenHash = token.sha1

        let paras: (string * obj) list =
            [ ("tokenHash", tokenHash) ]

        let n =
            dp.mkCmd().getFstVal (sql, paras)
            |> dp.managed.executeQuery

        match n with
        | Some x when x = 0 -> false
        //如果查询到的凭据记录唯一
        | Some x when coerce x = 1 ->
            //更新凭据访问记录
            let aff =
                dp
                    .mkCmd()
                    .update(table, ("atime", DateTime.Now), ("tokenHash", tokenHash))
                    .whenEq 1
                |> dp.managed.executeQuery

            if aff |> eq 1 then
                true
            else
                raise FailedToUpdateTokenAtime
        | _ -> raise DuplicateToken
