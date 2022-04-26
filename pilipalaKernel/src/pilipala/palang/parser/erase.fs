module pilipala.palang.parser.erase

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

let private erase_parse erase_clause =
    let pattern = "erase <type_name> <para_1>"

    let result =
        patternMatch pattern erase_clause |> unwrap

    let type_name = result.["type_name"]
    let para_1 = result.[""]

    match type_name with
    | "record" -> UInt64.Parse para_1 |> PostRecord.erase
    | "meta" -> UInt64.Parse para_1 |> PostMeta.erase
    | "comment" -> UInt64.Parse para_1 |> Comment.erase
    | "tag" -> para_1 |> tag.erase //此处为标签名
    | "token" -> para_1 |> token.erase //此处为凭据值
    | _ -> Err pilipala.palang.UnknownSyntax //未知语法错误
    |> unwrap

    $"{type_name} {para_1} was erased"
