namespace pilipala.db

open System.Data.Common
open System.Threading.Tasks
open System.Runtime.CompilerServices
open DbManaged
open DbManaged.PgSql
open fsharper.typ
open fsharper.typ.Ord

/// 数据库未初始化异常
(*
exception DbNotInitException

let mutable tablesResult: Result'<{| record: string
                                     meta: string
                                     comment: string
                                     token: string |}, exn> =
    Err DbNotInitException

let mutable managedResult: Result'<IDbManaged, exn> = Err DbNotInitException


type DbProvider
    (
        tables: {| record: string
                   meta: string
                   comment: string
                   token: string |},
        managed: IDbManaged
    ) =
    /// 表集合
    member self.tables = tables
    /// 管理器
    member self.managed = managed
    /// 命令行生成器
    member self.mkCmd() = managed.mkCmd ()
    
*)
[<Extension>]
type ext() =

    [<Extension>]
    static member inline alwaysCommit(f: (int -> bool) -> 'r) = always true |> f

    [<Extension>]
    static member inline whenEq(f: (int -> bool) -> 'r, n: int) = n |> eq |> f
(*
    [<Extension>]
    static member inline executeQuery(f: DbConnection -> 'r) = f |> managed.executeQuery

    [<Extension>]
    static member inline executeQueryAsync(f: DbConnection -> Task<'r>) = f |> managed.executeQueryAsync

    [<Extension>]
    static member inline queueQuery(f: DbConnection -> 'r) = f |> managed.queueQuery
*)
