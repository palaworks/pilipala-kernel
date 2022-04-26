module pilipala.palang.parser.tag

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

let private tag_parse tag_clause =
    let pattern = "tag <tag_name> to meta <meta_id>"

    let result =
        patternMatch pattern tag_clause |> unwrap

    let tag_name = result.["tag_name"]
    let meta_id = UInt64.Parse result.["meta_id"]

    tag.tagTo meta_id tag_name |> unwrap
    $"{tag_name} was tagged to meta {meta_id}"
