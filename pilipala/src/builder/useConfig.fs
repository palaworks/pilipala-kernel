[<AutoOpen>]
module pilipala.builder.useConfig

open System
open System.Collections.Generic
open System.Data.Common
open System.Reflection
open DbManaged
open DbManaged.PgSql
open Microsoft.FSharp.Core
open fsharper.op
open pilipala.container
open pilipala.util.yaml
open pilipala.util.json
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

  pooling:
    size: 32
    sync: 180

  map:
    meta: container.meta
    record: container.record
    comment: container.comment
    token: auth.token

plugin: ["./plugins/mailssage", "./plugins/llink"]

service: ["palang", "version", "shutdown"]

log: ["pala-std-output", "pala-std-error"]

auth:
  port: 20222
*)

type private dic<'k, 'v> = Dictionary<'k, 'v>

type Dictionary<'k, 'v> with
    member self.add(k: 'k, v: 'v) =
        self.Add(k, v)
        self

type Builder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member builder.useConfig(path: string) =
        let text = pilipala.util.io.readFile path
        let json = text.yamlInJson
        let root = json.jsonParsed

        dic<string, dic<string, obj>>()
            .add(
                "connection",
                dic<string, obj>()
                    .add("host", root.["connection"].["host"])
                    .add("port", root.["connection"].["port"])
                    .add("usr", root.["connection"].["usr"])
                    .add("pwd", root.["connection"].["pwd"])
                    .add ("using", root.["connection"].["using"])
            )
            .add(
                "pooling",
                dic<string, obj>()
                    .add("size", root.["pooling"].["size"])
                    .add ("sync", root.["pooling"].["sync"])
            )
            .add (
                "map",
                dic<string, obj>()
                    .add("meta", root.["map"].["meta"])
                    .add("record", root.["map"].["record"])
                    .add("comment", root.["map"].["comment"])
                    .add ("token", root.["map"].["token"])
            )
        |> builder.useDb
        |> ignore

        for path in root.["plugin"] do
            coerce path |> builder.usePlugin |> ignore

        for servName in root.["service"] do
            let a = Type.GetType(coerce servName)


            ()

        root.["auth"].["port"]
        |> coerce
        |> builder.useAuth
