namespace pilipala.db

open System.Collections.Generic
open System.Data.Common
open DbManaged

type DbProviderConsMsg =
    { config: Dictionary<string, Dictionary<string, obj>> }

type IDbProvider =

    /// 表集合
    abstract tables :
        {| record: string
           meta: string
           comment: string
           token: string |}

    /// 管理器
    abstract managed : IDbManaged

    /// 命令行生成器
    abstract mkCmd : unit -> DbCommand
