module internal pilipala.db

open DbManaged
open DbManaged.PgSql
open fsharper.types
open fsharper.types.Pipe.GenericPipable


/// 数据库连接信息
let mutable connMsg: Option'<DbConnMsg> = None

/// 连接池大小
let mutable private poolSize: Option'<uint> = None


/// 库名
let mutable private database: Option'<string> = None

/// 表集合
let mutable tables: Option'<{| record: string
                               meta: string
                               comment: string
                               token: string |}> =
    None

/// 管理器
let mutable private managed: Option'<IDbManaged> = None

let private initConfig () =
    let config = config.JsonConfig()
    let databaseNode = config.["database"] //database节点
    let tableNode = databaseNode.["table"] //database.table节点

    connMsg <-
        Some
        <| { Host = databaseNode.Value "host"
             Port = databaseNode.Value "port"
             User = databaseNode.Value "user"
             Password = databaseNode.Value "password" }

    poolSize <- Some <| databaseNode.Value "poolSize"

    //TODO：配置文件应从database节点独立出schema节点

    database <- Some <| databaseNode.Value "database"

    tables <-
        Some
        <| {| record = tableNode.Value "record"
              meta = tableNode.Value "meta"
              comment = tableNode.Value "comment"
              token = tableNode.Value "token" |}


let private schemaPipeline =

    let fetch () : IDbManaged =
        let _managed =
            PgSqlManaged(connMsg.unwrap (), database.unwrap (), poolSize.unwrap ())

        managed <- Some <| _managed

        _managed

    let provide () = managed.unwrap ()

    (GenericStatePipe(activate = initConfig, activated = id)
     |> GenericStatePipe(
         activate = fetch,
         activated = provide
     )
         .import)
        .build ()


let Managed = schemaPipeline.invoke
