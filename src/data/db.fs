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
              db = config.definition.name }

        let managed = new PgSqlManaged(msg, config.performance.pooling) :> IDbManaged

        { new IDbOperationBuilder with

            member i.makeCmd() = managed.makeCmd ()

            member i.execute f = managed.executeQuery f

            member i.executeQueryAsync f = managed.executeQueryAsync f

            member i.queue f = managed.queueQuery f

            member i.delay f = managed.delayQuery f

            member i.tables =
                {| user = config.definition.user
                   post = config.definition.post
                   comment = config.definition.comment |} }
