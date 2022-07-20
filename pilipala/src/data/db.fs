namespace pilipala.data.db

open System.Data.Common
open System.Threading.Tasks
open System.Runtime.CompilerServices
open DbManaged
open DbManaged.PgSql
open fsharper.typ
open fsharper.typ.Ord

(*
/// 数据库未初始化异常
exception DbNotInitException
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
