module pilipala.palang.parser.recycle

open System
open System.Collections.Generic
open pilipala.auth
open System.Text.RegularExpressions
open fsharper.op
open fsharper.types
open pilipala.container
open pilipala.container.post
open pilipala.container.comment
open pilipala.util.encoding
open pilipala.auth.channel

let private recycle_parse recycle_clause =
    let pattern = "recycle <type_name> <type_id>"

    let result =
        patternMatch pattern recycle_clause |> unwrap

    let type_name = result.["type_name"]
    let type_id = UInt64.Parse result.["type_id"]

    match type_name with
    | "meta" -> tag.tagTo type_id "invisible"
    | "comment" -> recycle<Comment, _, _> type_id
    | _ -> Err pilipala.kernel.palang.UnknownSyntax //未知语法错误
    |> unwrap

    $"{type_name} {type_id} was recycled"
