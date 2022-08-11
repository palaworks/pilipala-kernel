namespace pilipala.data.db

open DbManaged
open DbManaged.PgSql

module IDbOperationBuilder =
    let make (config: DbConfig) =
        let msg =
            { host = config.connection.host
              port = config.connection.port
              usr = config.connection.usr
              pwd = config.connection.pwd
              db = config.connection.using }

        let managed =
            new PgSqlManaged(msg, config.pooling.size) :> IDbManaged

        { new IDbOperationBuilder with
            member i.managed = managed

            member i.tables = config.map

            member i.makeCmd() = managed.mkCmd () }
