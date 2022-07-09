[<AutoOpen>]
module pilipala.builder.useConfig

open System
open System.Collections.Generic
open Newtonsoft.Json.Linq
open fsharper.op
open pilipala.util.yaml
open pilipala.util.json

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

plugin: ["./plugin/Mailssage", "./plugin/Llink"]

serv: ["./serv/Palang", "./serv/Version", "./serv/Shutdown"]

log:
  pilipala.serv.Auth: Information
  pilipala.serv.Palang: Information
  pilipala.serv.Version: Information
  pilipala.serv.Shutdown: Information

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

        //日志必须被首先配置，因为插件容器和服务容器都需要日志进行DI

        //注册日志过滤器
        for it in root.["log"] do
            let kv: JProperty = coerce it
            let category = kv.Name
            let lv = coerce kv.Value
            builder.useLogFilter category lv |> ignore
            
        //注册服务
        for dir in root.["serv"] do
            ((coerce dir): string)
            |> builder.useServ
            |> ignore

        //注册插件
        for dir in root.["plugin"] do
            ((coerce dir): string)
            |> builder.usePlugin
            |> ignore
            
        root.["auth"].["port"]
        |> coerce
        |> builder.useAuth
