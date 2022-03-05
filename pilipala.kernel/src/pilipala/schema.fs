module internal pilipala.schema

open MySqlManaged
open fsharper.moreType.GenericPipable
open fsharper.enhType


/// 数据库连接信息
let mutable connMsg: Option'<MySqlConnMsg> = None

/// 连接池大小
let mutable private poolSize: Option'<uint> = None


/// 数据库名
let mutable private name: Option'<string> = None


/// 数据库表
let mutable tables: Option'<{| record: string
                               meta: string
                               comment: string
                               token: string |}> =
    None

/// 管理器
let mutable private managed: Option'<MySqlManaged> = None

let private initConfig () =
    let config = config.JsonConfig()
    let database = config.["database"] //database节点
    let table = database.["table"] //database.table节点

    connMsg <-
        Some
        <| { DataSource = database.Value<string> "datasource"
             Port = database.Value<uint16> "port"
             User = database.Value<string> "user"
             Password = database.Value<string> "password" }

    poolSize <- Some <| database.Value<uint> "poolSize"

    //TODO：配置文件应从database节点独立出schema节点

    name <- Some <| database.Value<string> "schema"

    tables <-
        Some
        <| {| record = table.Value<string> "record"
              meta = table.Value<string> "meta"
              comment = table.Value<string> "comment"
              token = table.Value<string> "token" |}

let private initManaged () =

    let _managed =
        MySqlManaged(connMsg.unwarp (), name.unwarp (), poolSize.unwarp ())

    managed <- Some <| _managed

    _managed

let private provideManaged () = managed.unwarp ()

let private schemaPipeline =
    (GenericStatePipe(activate = initConfig, activated = id)
     |> GenericStatePipe(
         activate = initManaged,
         activated = provideManaged
     )
         .import)
        .build ()


let Managed = schemaPipeline.invoke
