module pilipala.palang.eval

open System
open System.Collections.Generic
open pilipala.auth
open System.Text.RegularExpressions
open fsharper.op
open fsharper.typ
open pilipala.container
open pilipala.container.Post
open pilipala.container.Comment
open pilipala.util.encoding
open pilipala.auth.channel

exception UnknownSyntax of msg: string

let eval_erase type_name para_1 =

    match type_name with
    | "record" -> UInt64.Parse para_1 |> post_record_entry.erase
    | "meta" -> UInt64.Parse para_1 |> post_meta_entry.erase
    | "comment" -> UInt64.Parse para_1 |> comment_entry.erase
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
        | "record" ->
            post_record_entry
                .create()
                .fmap (fun x -> x.ToString())
        | "meta" ->
            post_meta_entry
                .create()
                .fmap (fun x -> x.ToString())
        | "comment" ->
            comment_entry
                .create()
                .fmap (fun x -> x.ToString())
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
    | "comment" -> comment_entry.recycle type_id
    | _ -> Err(UnknownSyntax type_name) //未知语法错误
    |> unwrap

    $"{type_name} {type_id} was recycled"

let eval_set attribute type_name type_id base64url_attribute_value =
    let attribute_value =
        base64UrlToUtf8 base64url_attribute_value

    match type_name with
    | "record" ->
        match attribute with
        | "cover" -> (post_record_entry type_id).cover <- attribute_value
        | "title" -> (post_record_entry type_id).title <- attribute_value
        | "summary" -> (post_record_entry type_id).summary <- attribute_value
        | "body" -> (post_record_entry type_id).body <- attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | "meta" ->
        match attribute with
        | "view" -> (post_meta_entry type_id).view <- coerce attribute_value
        | "star" -> (post_meta_entry type_id).star <- coerce attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | "comment" ->
        match attribute with
        | "reply_to" -> (comment_entry type_id).replyTo <- coerce attribute_value
        | "nick" -> (comment_entry type_id).nick <- attribute_value
        | "content" -> (comment_entry type_id).content <- attribute_value
        | "email" -> (comment_entry type_id).email <- attribute_value
        | "site" -> (comment_entry type_id).site <- attribute_value
        | _ -> raise (UnknownSyntax attribute)
    | _ -> raise (UnknownSyntax type_name)

    $"the {attribute} of {type_name} {type_id} have been set"

let rec eval_rebase meta_id super_meta_id =

    (post_meta_entry meta_id).baseMetaId <- super_meta_id

    $"now meta {meta_id} is based on {super_meta_id}"

let eval_bind meta_id record_id =
    //TODO 此处应有合法性校验（上面的一大堆也是...
    (post_meta_entry meta_id).bindRecordId <- record_id

    $"now record {record_id} is bind to meta {meta_id}"
