module internal pilipala.schema

open fsharper.moreType.GenericPipable
open pilipala.database.mysql
open fsharper.ethType.ethOption
open pilipala.config


let mutable mysqlManaged: Option<MySqlManaged> = None

let initManaged () =
    let config = config ()

    let newMysqlManaged =
        MySqlManaged(config.databaseConnMsg, config.schemaName, config.connPoolSize)

    mysqlManaged <- newMysqlManaged |> Some

    newMysqlManaged

let provideManaged () = mysqlManaged.unwarp ()

let schemaPipeline =
    GenericStatePipe(activate = initManaged, activated = provideManaged)
        .build ()

let schema () = schemaPipeline.invoke ()
