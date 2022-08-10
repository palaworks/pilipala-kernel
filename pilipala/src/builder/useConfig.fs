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
      service: string list
      log: Dict<string, string>
      auth: {| port: u16 |} }

type Builder with

    /// 使用配置文件自动构造内核
    member self.useConfig(path: string) =
        let config =
            { yaml = readFile path }.deserializeTo<Config> ()

        self //acc is builder
        |> fun (acc: Builder) -> acc.useDb config.database
        //注册日志过滤器
        |> config.log.foldl (fun acc (KV (category, lv)) -> acc.useLoggerFilter category (coerce lv))
        //注册插件
        |> config.plugin.foldl (fun acc -> acc.useService)
        //注册服务
        |> config.service.foldl (fun acc -> acc.useService)
        |> fun acc -> acc.serveOn config.auth.port
