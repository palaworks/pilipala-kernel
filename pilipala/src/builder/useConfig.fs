[<AutoOpen>]
module pilipala.builder.useConfig

open System
open System.Collections.Generic
open fsharper.op
open fsharper.op.Alias
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
    member builder.useConfig(path: string) =
        let config =
            { yaml = readFile path }
                .intoJson()
                .deserializeTo<Config> ()

        builder.useDb config.database |> ignore

        //日志必须被首先配置，因为插件容器和服务容器都需要日志进行DI

        //注册日志过滤器
        for kv in config.log do
            let category = kv.Key
            let lv = coerce kv.Value
            builder.useLogFilter category lv |> ignore

        //注册服务
        for dir in config.serv do
            builder.useServ dir |> ignore

        //注册插件
        for dir in config.plugin do
            builder.usePlugin dir |> ignore

        builder.useAuth config.auth.port
