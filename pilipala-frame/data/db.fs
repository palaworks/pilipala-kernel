namespace pilipala.data.db

open System.Data.Common
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open DbManaged
open DbManaged.PgSql

type DbConfig =
    { connection: {| host: string //考虑到后续可能换用其他控制器，此处与DbManaged不作耦合
                     port: u16
                     usr: string
                     pwd: string
                     using: string |}
      pooling: {| size: u16; sync: u16 |}
      map: {| post: string
              comment: string
              token: string
              user: string |} }

type IDbOperationBuilder =

    /// 管理器
    abstract managed: IDbManaged

    /// 命令行生成器
    abstract makeCmd: unit -> DbCommand

    /// 表集合
    abstract tables:
        {| post: string
           comment: string
           token: string
           user: string |}

[<AutoOpen>]
module ext_IDbOperationBuilder =
    type IDbOperationBuilder with
        member db.Yield _ = db.makeCmd ()

    type IDbOperationBuilder with
        [<CustomOperation("inComment")>]
        member db.inComment cmd = cmd, db.tables.comment

        [<CustomOperation("inPost")>]
        member db.inPost cmd = cmd, db.tables.post

        [<CustomOperation("inToken")>]
        member db.inToken cmd = cmd, db.tables.token

        [<CustomOperation("inUser")>]
        member db.inUser cmd = cmd, db.tables.user

    type IDbOperationBuilder with
        [<CustomOperation("getFstVal")>]
        member db.getFstVal(cmd, sql, paras) = (cmd: DbCommand).getFstVal (sql, paras)

        [<CustomOperation("getFstVal")>]
        member db.getFstVal((cmd, table), targetKey, whereKey, whereVal) =
            (cmd: DbCommand)
                .getFstVal (table, targetKey, whereKey, whereVal)

        [<CustomOperation("getFstRow")>]
        member db.getFstRow((cmd, table), whereKey, whereVal) =
            (cmd: DbCommand)
                .getFstRow (table, whereKey, whereVal)


        [<CustomOperation("insert")>]
        member db.insert((cmd, table), fields) = (cmd: DbCommand).insert (table, fields)

        [<CustomOperation("select")>]
        member db.select(cmd, sql) = (cmd: DbCommand).select (sql)

        [<CustomOperation("update")>]
        member db.update((cmd, table), targetKey, targetVal, whereKey, whereVal) =
            (cmd: DbCommand)
                .update (table, (targetKey, targetVal), (whereKey, whereVal))

        [<CustomOperation("delete")>]
        member db.delete((cmd, table), whereKey, whereVal) =
            (cmd: DbCommand)
                .delete (table, whereKey, whereVal)

    type IDbOperationBuilder with
        [<CustomOperation("whenEq")>]
        member db.whenEq(f, n) = f (eq n)

        [<CustomOperation("alwaysCommit")>]
        member db.always f = f (always true)

    type IDbOperationBuilder with
        [<CustomOperation("execute")>]
        member db.execute f = db.managed.executeQuery f

        [<CustomOperation("executeAsync")>]
        member db.executeQueryAsync f = db.managed.executeQueryAsync f

        [<CustomOperation("queue")>]
        member db.queueQuery f = db.managed.queueQuery f

        [<CustomOperation("delay")>]
        member db.delayQuery f = db.managed.queueQuery f
