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

            member i.makeCmd() = managed.mkCmd ()

            member i.execute f = managed.executeQuery f

            member i.executeQueryAsync f = managed.executeQueryAsync f

            member i.queue f = managed.queueQuery f

            member i.delay f = managed.delayQuery f

            member i.tables = config.map }
