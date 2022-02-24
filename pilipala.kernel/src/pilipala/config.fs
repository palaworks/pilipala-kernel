module internal pilipala.config

open System.IO
open fsharper.ethType.ethOption
open pilipala.util.yaml
open Newtonsoft.Json.Linq
open fsharper.moreType.GenericPipable

//配置文件路径
let mutable configFilePath: Option<string> = None

//数据库连接信息
let mutable databaseConnMsg: Option<{| DataSource: string
                                       Port: uint16
                                       User: string
                                       Password: string |}> =
    None


//连接池大小
let mutable connPoolSize: Option<uint> = None

//数据库名
let mutable schemaName: Option<string> = None

//数据库表
let mutable schemaTable: Option<{| record: string
                                   stack: string
                                   comment: string
                                   token: string |}> =
    None




let fetchConfig () =
    let config =
        File.ReadAllText(configFilePath.unwarp (), System.Text.Encoding.UTF8)

    let root = config.yamlInJson |> JObject.Parse
    let database = root.["database"] //database节点
    let table = database.["table"] //database.table节点

    let _databaseConnMsg =
        {| DataSource = database.Value<string> "dataSource"
           Port = database.Value<uint16> "port"
           User = database.Value<string> "user"
           Password = database.Value<string> "password" |}

    let _connPoolSize = database.Value<uint> "poolSize"

    //TODO：配置文件应从database节点独立出schema节点

    let _schemaName = database.Value<string> "schema"

    let _schemaTable =
        {| record = table.Value<string> "record"
           stack = table.Value<string> "stack"
           comment = table.Value<string> "comment"
           token = table.Value<string> "token" |}

    databaseConnMsg <- Some <| _databaseConnMsg
    connPoolSize <- Some <| _connPoolSize
    schemaName <- Some <| _schemaName
    schemaTable <- Some <| _schemaTable

    {| databaseConnMsg = _databaseConnMsg
       connPoolSize = _connPoolSize
       schemaName = _schemaName
       schemaTable = _schemaTable |}


let provideConfig () =
    {| databaseConnMsg = databaseConnMsg.unwarp ()
       connPoolSize = connPoolSize.unwarp ()
       schemaName = schemaName.unwarp ()
       schemaTable = schemaTable.unwarp () |}

let configPipeline =
    GenericStatePipe(activate = fetchConfig, activated = provideConfig)
        .build ()

let config = configPipeline.invoke
