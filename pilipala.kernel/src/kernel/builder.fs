[<AutoOpen>]
module pilipala.launcher

open System
open Newtonsoft.Json.Linq
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open pilipala.util.yaml
open pilipala.database.mysql

type pilipala private (table, database) =
    ///内核单例
    static let mutable kernel' = None
    ///内核单例访问器
    static member kernel
        with public get () = kernel'
        and private set v = kernel' <- v

    member this.table = table
    member this.database = database

    ///启动内核
    static member start(config: string) =
        match pilipala.kernel with
        | None ->
            let root = config.yamlInJson |> JObject.Parse

            let database = root.["database"] //database节点
            let table = database.["table"] //database.table节点

            let msg = //连接信息
                {| DataSource = database.Value<string> "dataSource"
                   Port = database.Value<uint16> "port"
                   User = database.Value<string> "user"
                   Password = database.Value<string> "password" |}

            let poolSz = database.Value<uint> "poolSize" //连接池大小
            let schema = database.Value<string> "schema" //数据库

            let table =
                {| record = table.Value<string> "record"
                   stack = table.Value<string> "stack"
                   comment = table.Value<string> "comment"
                   token = table.Value<string> "token" |}

            let manager = MySqlManager(msg, schema, poolSz)

            pilipala.kernel <- pilipala (table, manager) |> Some
        | Some _ -> ()


/// 内核未初始化错误
exception KernelUninitialized

/// 取得内核单例
let pala () =
    match pilipala.kernel with
    | Some k -> Ok k
    | None -> Err KernelUninitialized

/// 重复构造配置
exception Config

type KernelBuilder() =
    let mutable config = ""
    /// 使用配置
    member self.useConfig(s: string) =
        if config <> "" then config <- s else ()
    /// 使用配置（惰性求值）
    member self.useConfig(f: unit -> string) = if config <> "" then () else ()
    /// 使用全局缓存
    member self.useGlobalCache() = ()
    /// 使用页缓存
    member self.usePageCache() = ()
    /// 使用内存表
    member self.useMemoryTable() = ()
    /// 使用调试信息
    member self.useDebugMessage() = ()
    /// 使用插件
    member self.usePlugin plugin = ()
    /// 使用日志记录
    member self.useLog logPath = ()
    /// 使用认证系统
    member self.useAuth port service = ()
    /// 构建
    member self.Build() = 0

let K = KernelBuilder()
