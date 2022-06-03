[<AutoOpen>]
module pilipala.builder.useDb

open System.Collections.Generic
open System.Data.Common
open DbManaged
open DbManaged.PgSql
open fsharper.op
open pilipala
open fsharper.typ.Pipe.Pipable
open fsharper.op.Alias
open fsharper.op.Coerce
open fsharper.typ

type palaBuilder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDbConfig (configDic:Dictionary<string,obj>) =
        let func _ =
            let msg =
                { Host = coerce configDic["host"]
                  Port = coerce configDic["port"]
                  User = coerce configDic["user"]
                  Password = coerce configDic["pwd"] }
                
            db.tablesResult <-
                Ok <|
                    {|
                        record = coerce configDic["record"]
                        meta = coerce configDic["meta"]
                        comment = coerce configDic["comment"]
                        token = coerce configDic["token"]
                    |}
            db.managedResult <-Ok <| new PgSqlManaged(msg,coerce configDic["dbName"],coerce configDic["poolSize"])
            taskQueue.queueTask <- fun f -> f () |> ignore

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
