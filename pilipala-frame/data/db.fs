namespace pilipala.data.db

open System.Collections.Generic
open fsharper.op.Alias
open System.Data.Common
open DbManaged

type DbProviderConsMsg =
    { connection: {| host: string
                     port: u32
                     usr: string
                     pwd: string
                     using: string |}
      pooling: {| size: u32; sync: u32 |}
      map: {| post: string
              comment: string
              token: string
              usr: string |} }

type IDbProvider =

    /// 管理器
    abstract managed: IDbManaged

    /// 命令行生成器
    abstract mkCmd: unit -> DbCommand

    /// 表集合
    abstract tables:
        {| record: string
           meta: string
           comment: string
           token: string |}
