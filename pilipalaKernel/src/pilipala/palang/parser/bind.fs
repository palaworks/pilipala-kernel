module pilipala.palang.parser.bind

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

let private bind_parse bind_clause =
    let pattern =
        "bind record <record_id> to meta <meta_id>"

    let result =
        patternMatch pattern bind_clause |> unwrap

    let meta_id = UInt64.Parse result.["meta_id"]
    let record_id = UInt64.Parse result.["record_id"]

    (PostMeta meta_id).currRecordId <- record_id

    $"now record {record_id} is bind to meta {meta_id}"
