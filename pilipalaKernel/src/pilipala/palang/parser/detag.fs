module pilipala.palang.parser.detag

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

let private detag_parse detag_clause =
    let pattern = "detag <tag_name> for meta <meta_id>"

    let result =
        patternMatch pattern detag_clause |> unwrap

    let tag_name = result.["tag_name"]
    let meta_id = UInt64.Parse result.["meta_id"]

    tag.detagFor meta_id tag_name |> unwrap
    $"tag {tag_name} now removed from meta {meta_id}"
