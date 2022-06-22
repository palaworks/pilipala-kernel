[<AutoOpen>]
module pilipala.builder.useDb

open System.Collections.Generic
open System.Data.Common
open DbManaged
open DbManaged.PgSql
open fsharper.op
open pilipala
open fsharper.typ.Pipe.Pipable
open fsharper.op.Alias
open fsharper.op.Coerce
open fsharper.typ

(*
database:
  connection:
    host: localhost
    port: 3306

    usr: root
    pwd: 65a1561425f744e2b541303f628963f8

    using: pilipala_fs

  map:
    meta: container.meta
    record: container.record
    comment: container.comment
    token: auth.token

  pooling:
    size: 32
    sync: 180

plugin: ["./plugins/mailssage", "./plugins/llink"]

auth:
  port: 20222
*)

type Builder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDb(config: Dictionary<string, Dictionary<string, obj>>) =
        let func _ =
            let msg =
                { host = coerce config.["connection"].["host"]
                  port = coerce config.["connection"].["host"]
                  usr = coerce config.["connection"].["host"]
                  pwd = coerce config.["connection"].["pwd"]
                  db = coerce config.["connection"].["using"] }

            db.tablesResult <-
                Ok
                <| {| meta = coerce config.["map"].["meta"]
                      record = coerce config.["map"].["record"]
                      comment = coerce config.["map"].["comment"]
                      token = coerce config.["map"].["token"] |}

            db.managedResult <-
                Ok
                <| new PgSqlManaged(msg, coerce config.["pooling"].["size"])

        self.buildPipeline.mappend (Pipe(func = func))
