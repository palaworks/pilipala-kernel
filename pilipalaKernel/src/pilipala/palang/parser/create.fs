module pilipala.palang.parser.create

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

let private create_parse create_clause =
    let pattern = "create <type_name> [tag_name]"

    let result =
        patternMatch pattern create_clause |> unwrap

    let type_name = result.["type_name"]

    let name, (value: string) =
        match type_name with
        | "record" -> "id", PostRecord.create().unwrap().ToString()
        | "meta" -> "id", PostMeta.create().unwrap().ToString()
        | "comment" -> "id", Comment.create().unwrap().ToString()
        | "tag" ->
            let tag_name = result.["tag_name"]
            "name", tag.create tag_name |> unwrap
        | "token" -> "value", token.create () |> unwrap
        | _ -> raise pilipala.kernel.palang.UnknownSyntax //未知语法错误

    $"new {type_name} was created with {name} {value}"
