namespace pilipala.kernel

open System
open Newtonsoft.Json.Linq
open pilipala.data
open pilipala.util.yaml
open pilipala.database.mysql

[<AutoOpen>]
module launcher =

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
