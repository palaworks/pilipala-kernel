module pilipala.palang.parser.set

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

let private set_parse set_clause =
    let pattern =
        "set <attribute> for <type_name> <type_id> to <base64url_attribute_value>"

    let result =
        patternMatch pattern set_clause |> unwrap

    let attribute = result.["attribute"]
    let type_name = result.["type_name"]
    let type_id = UInt64.Parse result.["type_id"]

    let attribute_value =
        decodeBase64url result.["base64url_attribute_value"]

    match type_name with
    | "record" ->
        match attribute with
        | "cover" ->
            Ok
            <| (PostRecord type_id).cover <- attribute_value
        | "title" ->
            Ok
            <| (PostRecord type_id).title <- attribute_value
        | "summary" ->
            Ok
            <| (PostRecord type_id).summary <- attribute_value
        | "body" -> Ok <| (PostRecord type_id).body <- attribute_value
        | _ -> Err pilipala.palang.UnknownSyntax
    | "meta" ->
        match attribute with
        | "view" ->
            Ok
            <| (PostMeta type_id).view <- coerce attribute_value
        | "star" ->
            Ok
            <| (PostMeta type_id).star <- coerce attribute_value
        | _ -> Err pilipala.palang.UnknownSyntax
    | "comment" ->
        match attribute with
        | "reply_to" ->
            Ok
            <| (Comment type_id).replyTo <- coerce attribute_value
        | "nick" -> Ok <| (Comment type_id).nick <- attribute_value
        | "content" -> Ok <| (Comment type_id).content <- attribute_value
        | "email" -> Ok <| (Comment type_id).email <- attribute_value
        | "site" -> Ok <| (Comment type_id).site <- attribute_value
        | _ -> Err pilipala.palang.UnknownSyntax
    | _ -> Err pilipala.palang.UnknownSyntax
    |> unwrap

    $"the {attribute} of {type_name} {type_id} have been set"
