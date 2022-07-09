[<AutoOpen>]
module pilipala.builder.useDb

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open DbManaged
open DbManaged.PgSql
open Microsoft.Extensions.DependencyInjection
open pilipala
open pilipala.db

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
            self.DI.AddSingleton<DbProviderConsMsg>(fun _ -> { config = config })
            |> ignore

        self.buildPipeline.export (StatePipe(activate = func))
