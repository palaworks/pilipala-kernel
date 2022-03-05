module pilipala.kernel.palang

open System
open pilipala.auth
open System.Text.RegularExpressions
open fsharper.fn
open fsharper.op
open fsharper.enhType
open pilipala.container
open pilipala.container.post
open pilipala.container.comment
open pilipala.util.encoding
open pilipala.auth.channel


//TODO 有待减少样板代码
/// 各类命令解析

/// 未知语法错误
exception UnknownSyntax

let private create_parse (argv: string array) =
    //   0        1          2
    //create <type_name> [tag_name]

    let type_name = argv.[1]

    let name, value =
        match argv.[1] with
        | "record" -> "id", PostRecord.create () <%> cast |> unwarp
        | "meta" -> "id", PostMeta.create () <%> cast |> unwarp
        | "comment" -> "id", create<Comment, _> <%> cast |> unwarp
        | "tag" when argv.Length = 3 ->
            let tag_name = argv.[2]
            "name", tag.create tag_name |> unwarp
        | "token" -> "value", token.create () |> unwarp
        | _ -> raise UnknownSyntax //未知语法错误

    $"new {type_name} was created with {name} {value}"

let private recycle_parse (argv: string array) =
    //   0         1          2
    //recycle <type_name> <type_id>

    let type_name = argv.[1]
    let type_id = argv.[2] |> cast

    match type_name with
    | "meta" -> tag.tagTo type_id "invisible"
    | "comment" -> recycle<Comment, _, _> type_id
    | _ -> Err UnknownSyntax //未知语法错误
    |> unwarp

    $"{type_name} {type_id} was recycled"

let private erase_parse (argv: string array) =
    //  0        1       2
    //erase <type_name> <1>

    let type_name = argv.[1]
    let type_id = argv.[2] |> cast

    match type_name with
    | "record" -> type_id |> PostRecord.erase
    | "meta" -> type_id |> PostMeta.erase
    | "comment" -> type_id |> Comment.erase
    | "tag" -> type_id |> cast |> tag.erase //type_id此处为标签名
    | "token" -> type_id |> cast |> token.erase //type_id此处为凭据值
    | _ -> Err UnknownSyntax //未知语法错误
    |> unwarp

    $"{type_name} {type_id} was successfully erased"

let private tag_parse (argv: string array) =
    // 0      1       2   3       4
    //tag <tag_name> to meta <meta_id>

    let tagName = argv.[1].ToLower()
    let metaId = argv.[4] |> cast

    tag.tagTo metaId tagName |> unwarp
    $"{tagName} was tagged to meta {metaId}"

let private detag_parse (argv: string array) =
    // 0         1      2    3       4
    //detag <tag_name> for meta <meta_id>

    let tagName = argv.[1]
    let metaId = argv.[4] |> cast

    tag.detagFor metaId tagName |> unwarp
    $"tag {tagName} now removed from meta {metaId}"



let private set_parse (argv: string array) =
    // 0       1       2       3          4      5             6
    //set <attribute> for <type_name> <type_id> to <base64url_attribute_value>

    let attribute = argv.[1]
    let type_name = argv.[3]
    let type_id = argv.[4] |> cast
    let attribute_value = decodeBase64url argv.[6]

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
        | _ -> Err UnknownSyntax
    | "meta" ->
        match attribute with
        | "view" ->
            Ok
            <| (PostMeta type_id).view <- cast <| attribute_value
        | "star" ->
            Ok
            <| (PostMeta type_id).star <- cast <| attribute_value
        | _ -> Err UnknownSyntax
    | "comment" ->
        match attribute with
        | "reply_to" ->
            Ok
            <| (Comment type_id).replyTo <- cast <| attribute_value
        | "nick" -> Ok <| (Comment type_id).nick <- attribute_value
        | "content" -> Ok <| (Comment type_id).content <- attribute_value
        | "email" -> Ok <| (Comment type_id).email <- attribute_value
        | "site" -> Ok <| (Comment type_id).site <- attribute_value
        | _ -> Err UnknownSyntax
    | _ -> Err UnknownSyntax
    |> unwarp

    $"the {attribute} of {type_name} {type_id} have been set"

let private rebase_parse (argv: string array) =
    //   0       1       2        3
    //rebase <meta_id> to <super_meta_id>

    let meta_id = argv.[1] |> cast
    let super_meta_id = argv.[3] |> cast

    (PostMeta meta_id).superMetaId <- super_meta_id

    $"now meta {meta_id} is derived from {super_meta_id}"

let private push_parse (argv: string array) =
    //  0     1        2        3    4       5
    //push record <record_id> into meta <meta_id>

    let record_id = cast <| argv.[2]
    let meta_id = cast <| argv.[5]

    (PostMeta meta_id).currRecordId <- record_id

    $"now the top of meta {meta_id} is record {record_id}"


let internal parse (cmd: string) =

    //除空格作为分隔符外，palang只支持_A-Za-z0-9和+/*-（用于base64及base64url）
    //下面的举措有利于去除各种非显示字符和不受palang支持的字符（它们通常是在各种解码过程中产生的）
    let argv = //将其他字符合并为空格、首尾去空格
        Regex
            .Replace(cmd, "[^_\w+/*-]+", " ")
            .Trim()
            .Split(' ')

    let argc = argv.Length

    try
        argv //Split(' ')结果长度不可能小于1
        |> match argc, argv.[0] with
           //通用
           | 2, "create"
           | 3, "create" -> create_parse
           | 3, "recycle" -> recycle_parse
           | 3, "erase" -> erase_parse
           //属性设置
           | 7, "set" -> set_parse
           //文章元
           | 4, "rebase" -> rebase_parse
           | 6, "push" -> push_parse
           //标签
           | 5, "tag" -> tag_parse
           | 5, "detag" -> detag_parse
           //未知语法
           | _ -> konst "unknown syntax"
    with
    | UnknownSyntax -> "unknown syntax"
    | FailedToWriteCache -> "op failed"
    | e -> "op failed with : " + e.Message

let palangService (channel: SecureChannel) =
    let log msg =
        Console.WriteLine $"palang service : {msg}"

    log "online"

    while true do //持续执行命令
        let cmd = channel.recvText ()

        log $"command received < {cmd}"

        let result = parse cmd

        channel.sendText result

        log $"command executed > {result}"
