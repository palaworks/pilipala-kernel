module pilipala.palang.eval

open System
open System.Collections.Generic
open pilipala.auth
open System.Text.RegularExpressions
open fsharper.op
open fsharper.typ
open pilipala.container
open pilipala.container.post
open pilipala.container.comment
open pilipala.util.encoding
open pilipala.auth.channel

exception UnknownSyntax of msg: string

let eval_erase type_name para_1 =

    match type_name with
    | "record" -> UInt64.Parse para_1 |> PostRecord.erase
    | "meta" -> UInt64.Parse para_1 |> PostMeta.erase
    | "comment" -> UInt64.Parse para_1 |> Comment.erase
    | "tag" -> para_1 |> tag.erase //此处为标签名
    | "token" -> para_1 |> token.erase //此处为凭据值
    | _ -> Err(UnknownSyntax type_name) //未知语法错误
    |> unwrap

    $"{type_name} {para_1} was erased"

let eval_detag tag_name meta_id =
    tag.detagFor meta_id tag_name |> unwrap

    $"tag {tag_name} now removed from meta {meta_id}"

let eval_tag tag_name meta_id =

    tag.tagTo meta_id tag_name |> unwrap

    $"{tag_name} was tagged to meta {meta_id}"

let eval_create type_name tag_name =

    let value =
        match type_name with
        | "record" -> PostRecord.create().fmap (fun x -> x.ToString())
        | "meta" -> PostMeta.create().fmap (fun x -> x.ToString())
        | "comment" -> Comment.create().fmap (fun x -> x.ToString())
        | "tag" -> tag_name |> force |> tag.create
        | "token" -> token.create ()
        | _ -> Err(UnknownSyntax type_name) //未知语法错误
        |> unwrap

    let name =
        match type_name with
        | "record" -> "id"
        | "meta" -> "id"
        | "comment" -> "id"
        | "tag" -> "name"
        | "token" -> "value"
        | _ -> ""

    $"new {type_name} was created with {name} {value}"

let eval_recycle type_name type_id =

    match type_name with
    | "meta" -> tag.tagTo type_id "invisible"
    | "comment" -> recycle<Comment, _, _> type_id
    | _ -> Err(UnknownSyntax type_name) //未知语法错误
    |> unwrap

    $"{type_name} {type_id} was recycled"

let eval_set attribute type_name type_id base64url_attribute_value =
    let attribute_value =
        base64UrlToUtf8 base64url_attribute_value

    match type_name with
    | "record" ->
        match attribute with
        | "cover" -> (PostRecord type_id).cover <- attribute_value
        | "title" -> (PostRecord type_id).title <- attribute_value
        | "summary" -> (PostRecord type_id).summary <- attribute_value
        | "body" -> (PostRecord type_id).body <- attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | "meta" ->
        match attribute with
        | "view" -> (PostMeta type_id).view <- coerce attribute_value
        | "star" -> (PostMeta type_id).star <- coerce attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | "comment" ->
        match attribute with
        | "reply_to" -> (Comment type_id).replyTo <- coerce attribute_value
        | "nick" -> (Comment type_id).nick <- attribute_value
        | "content" -> (Comment type_id).content <- attribute_value
        | "email" -> (Comment type_id).email <- attribute_value
        | "site" -> (Comment type_id).site <- attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | _ -> raise (UnknownSyntax type_name)

    $"the {attribute} of {type_name} {type_id} have been set"

let rec eval_rebase meta_id super_meta_id =

    (PostMeta meta_id).superMetaId <- super_meta_id

    $"now meta {meta_id} is based on {super_meta_id}"

let eval_bind meta_id record_id =
    (PostMeta meta_id).currRecordId <- record_id

    $"now record {record_id} is bind to meta {meta_id}"
