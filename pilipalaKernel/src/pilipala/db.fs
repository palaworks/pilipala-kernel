module internal pilipala.db

open DbManaged
open DbManaged.PgSql
open fsharper.typ
open fsharper.typ.Pipe.GenericPipable

/// 数据库未初始化异常
exception DbNotInitException

/// 数据库连接信息
let mutable connMsg: Result'<DbConnMsg, exn> = Err DbNotInitException

/// 连接池大小
let mutable private poolSize: Result'<uint, exn> = Err DbNotInitException

/// 库名
let mutable private database: Result'<string, exn> = Err DbNotInitException

/// 表集合
let mutable tables: Result'<{| record: string
                               meta: string
                               comment: string
                               token: string |}, exn> =
    Err DbNotInitException

/// 管理器
let mutable private managed: Option'<IDbManaged> = None

let private initConfig () =
    let config = config.JsonConfig()
    let databaseNode = config.["database"] //database节点
    let tableNode = databaseNode.["table"] //database.table节点

    connMsg <-
        Ok
        <| { Host = databaseNode.Value "host"
             Port = databaseNode.Value "port"
             User = databaseNode.Value "user"
             Password = databaseNode.Value "password" }

    poolSize <- Ok <| databaseNode.Value "poolSize"

    //TODO：配置文件应从database节点独立出schema节点

    database <- Ok <| databaseNode.Value "database"

    tables <-
        Ok
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
