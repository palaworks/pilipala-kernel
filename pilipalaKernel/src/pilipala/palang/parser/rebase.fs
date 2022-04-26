module pilipala.palang.parser.rebase

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

let private rebase_parse rebase_clause =
    let pattern = "rebase <meta_id> to <super_meta_id>"

    let result =
        patternMatch pattern rebase_clause |> unwrap

    let meta_id = UInt64.Parse result.["meta_id"]
    let super_meta_id = UInt64.Parse result.["super_meta_id"]

    (PostMeta meta_id).superMetaId <- super_meta_id

    $"now meta {meta_id} is based on {super_meta_id}"
