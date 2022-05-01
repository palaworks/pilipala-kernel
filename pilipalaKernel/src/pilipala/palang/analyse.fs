module pilipala.palang.analyse

open System
open System.Collections.Generic
open pilipala.auth
open System.Text.RegularExpressions
open fsharper.op
open fsharper.types
open pilipala.container
open pilipala.container.post
open pilipala.container.comment
open pilipala.palang
open pilipala.util.encoding
open pilipala.palang.util
open pilipala.palang.eval


let analyse_erase erase_clause =
    let pattern = "erase <type_name> <para_1>"

    let result =
        patternMatch pattern erase_clause |> unwrap

    let type_name = result.["type_name"]
    let para_1 = result.["para_1"]

    eval_erase type_name para_1

let analyse_detag detag_clause =
    let pattern = "detag <tag_name> for meta <meta_id>"

    let result =
        patternMatch pattern detag_clause |> unwrap

    let tag_name = result.["tag_name"]
    let meta_id = UInt64.Parse result.["meta_id"]

    eval_detag tag_name meta_id

let analyse_tag tag_clause =
    let pattern = "tag <tag_name> to meta <meta_id>"

    let result =
        patternMatch pattern tag_clause |> unwrap

    let tag_name = result.["tag_name"]
    let meta_id = UInt64.Parse result.["meta_id"]

    eval_tag tag_name meta_id

let analyse_create create_clause =
    let pattern = "create <type_name> [tag_name]"

    let result =
        patternMatch pattern create_clause |> unwrap

    let type_name = result.["type_name"]

    eval_create type_name (lazy (result.["tag_name"]))

let analyse_recycle recycle_clause =
    let pattern = "recycle <type_name> <type_id>"

    let result =
        patternMatch pattern recycle_clause |> unwrap

    let type_name = result.["type_name"]
    let type_id = UInt64.Parse result.["type_id"]

    eval_recycle type_name type_id

let analyse_set set_clause =
    let pattern =
        "set <attribute> for <type_name> <type_id> to <base64url_attribute_value>"

    let result =
        patternMatch pattern set_clause |> unwrap

    let attribute = result.["attribute"]
    let type_name = result.["type_name"]
    let type_id = UInt64.Parse result.["type_id"]

    let base64url_attribute_value = result.["base64url_attribute_value"]


    eval_set attribute type_name type_id base64url_attribute_value

let analyse_rebase rebase_clause =
    let pattern = "rebase <meta_id> to <super_meta_id>"

    let result =
        patternMatch pattern rebase_clause |> unwrap

    let meta_id = UInt64.Parse result.["meta_id"]
    let super_meta_id = UInt64.Parse result.["super_meta_id"]

    eval_rebase meta_id super_meta_id

let analyse_bind bind_clause =
    let pattern =
        "bind record <record_id> to meta <meta_id>"

    let result =
        patternMatch pattern bind_clause |> unwrap

    let meta_id = UInt64.Parse result.["meta_id"]
    let record_id = UInt64.Parse result.["record_id"]

    eval_bind meta_id record_id


let analyse_cmd (cmd: string) =

    //除空格作为分隔符外，palang只支持_A-Za-z0-9和+/*-（用于base64及base64url）
    //下面的举措有利于去除各种非显示字符和不受palang支持的字符（它们通常是在各种解码过程中产生的）
    let processedCmd =
        Regex.Replace(cmd, "[^_\w+/*-]+", " ").Trim() //将其他字符合并为空格、首尾去空格

    try
        processedCmd //Split(' ')结果长度不可能小于1
        |> match processedCmd.Split(' ').[0] with
           //通用
           | "create" -> analyse_create
           | "recycle" -> analyse_recycle
           | "erase" -> analyse_erase
           //属性设置
           | "set" -> analyse_set
           //元信息设置
           | "rebase" -> analyse_rebase
           | "bind" -> analyse_bind
           //标签
           | "tag" -> analyse_tag
           | "detag" -> analyse_detag
           //未知语法
           | _ -> raise (UnknownSyntax processedCmd)
    with
    | e -> "op failed with : " + e.Message
