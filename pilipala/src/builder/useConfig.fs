[<AutoOpen>]
module pilipala.builder.useConfig

open System.Collections.Generic
open fsharper.op
open fsharper.alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.util.io
open pilipala.util.text

type Dictionary<'k, 'v> with
    member self.add(k: 'k, v: 'v) =
        self.Add(k, v)
        self

type Config =
    { database: DbConfig
      plugin: string list
      serv: string list
      log: Dict<string, string>
      auth: {| port: u16 |} }

type Builder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useConfig(path: string) =
        let config =
            { yaml = readFile path }.deserializeTo<Config> ()

        self
        |> fun (acc: Builder) -> acc.useDb config.database
        //日志必须被首先配置，因为插件容器和服务容器都需要日志进行DI
        //注册日志过滤器
        |> config.log.foldl (fun acc (KV (category, lv)) -> acc.useLoggerFilter category (coerce lv))
        //注册服务
        |> config.serv.foldl (fun acc -> acc.useService)
        //注册插件
        |> config.plugin.foldl (fun acc -> acc.useService)
        |> fun acc -> acc.useAuth config.auth.port
