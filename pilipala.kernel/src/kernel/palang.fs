namespace pilipala.kernel

open System.Threading.Channels
open pilipala.kernel.auth

module palang =

    //TODO 有待减少样板代码
    /// 各类命令解析

    open System.Text.RegularExpressions
    open pilipala.data
    open pilipala.container
    open pilipala.util.encoding
    open pilipala.kernel
    open System

    //open System

    /// 未知语法错误
    exception UnknownSyntax

    let private create_parse (argv: string array) =
        //   0        1          2
        //create <type_name> [tag_name]

        let type_name = argv.[1]

        let name, value =
            match argv.[1] with
            | "record" -> "id", post.record.create () <%> cast |> unwarp
            | "stack" -> "id", post.stack.create () <%> cast |> unwarp
            | "comment" -> "id", comment.create () <%> cast |> unwarp
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
        | "stack" -> tag.tagTo type_id "invisible"
        | "comment" -> comment.recycle type_id
        | _ -> Err UnknownSyntax //未知语法错误
        |> unwarp

        $"{type_name} {type_id} was recycled"

    let private erase_parse (argv: string array) =
        //  0        1       2
        //erase <type_name> <1>

        let type_name = argv.[1]
        let type_id = argv.[2]

        match type_name with
        | "record" -> type_id |> cast |> post.record.erase
        | "stack" -> type_id |> cast |> post.stack.erase
        | "comment" -> type_id |> cast |> comment.erase
        | "tag" -> type_id |> tag.erase //type_id此处为标签名
        | "token" -> type_id |> token.erase //type_id此处为凭据值
        | _ -> Err UnknownSyntax //未知语法错误
        |> unwarp

        $"{type_name} {type_id} was successfully erased"

    let private tag_parse (argv: string array) =
        // 0      1       2   3       4
        //tag <tag_name> to stack <stack_id>

        let tagName = argv.[1].ToLower()
        let stackId = argv.[4] |> cast

        tag.tagTo stackId tagName |> unwarp
        $"{tagName} was tagged to stack {stackId}"

    let private detag_parse (argv: string array) =
        // 0         1      2    3       4
        //detag <tag_name> for stack <stack_id>

        let tagName = argv.[1]
        let stackId = argv.[4] |> cast

        tag.detagFor stackId tagName |> unwarp
        $"tag {tagName} now removed from stack {stackId}"


    //用于减少样板代码
    type private record = post.record.PostRecord
    type private stack = post.stack.PostStack
    type private comment = comment.Comment

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
            | "cover" -> Ok <| (record type_id).cover <- attribute_value
            | "title" -> Ok <| (record type_id).title <- attribute_value
            | "summary" -> Ok <| (record type_id).summary <- attribute_value
            | "body" -> Ok <| (record type_id).body <- attribute_value
            | _ -> Err UnknownSyntax
        | "stack" ->
            match attribute with
            | "view" ->
                Ok
                <| (stack type_id).view <- cast <| attribute_value
            | "star" ->
                Ok
                <| (stack type_id).star <- cast <| attribute_value
            | _ -> Err UnknownSyntax
        | "comment" ->
            match attribute with
            | "reply_to" ->
                Ok
                <| (comment type_id).replyTo <- cast <| attribute_value
            | "nick" -> Ok <| (comment type_id).nick <- attribute_value
            | "content" -> Ok <| (comment type_id).content <- attribute_value
            | "email" -> Ok <| (comment type_id).email <- attribute_value
            | "site" -> Ok <| (comment type_id).site <- attribute_value
            | _ -> Err UnknownSyntax
        | _ -> Err UnknownSyntax
        |> unwarp

        $"the {attribute} of {type_name} {type_id} have been set"

    let private rebase_parse (argv: string array) =
        //   0       1       2        3
        //rebase <stack_id> to <super_stack_id>

        let stack_id = argv.[1] |> cast
        let super_stack_id = argv.[3] |> cast

        (stack stack_id).superStackId <- super_stack_id

        $"now stack {stack_id} is derived from {super_stack_id}"

    let private push_parse (argv: string array) =
        //  0     1        2        3    4       5
        //push record <record_id> into stack <stack_id>

        let record_id = cast <| argv.[2]
        let stack_id = cast <| argv.[5]

        (stack stack_id).currRecordId <- record_id

        $"now the top of stack {stack_id} is record {record_id}"


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
               //文章栈
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
        let cli msg =
            Console.WriteLine $"palang service : {msg}"

        cli "online"

        while true do //持续执行命令
            let cmd = channel.recv ()

            cli $"command received < {cmd}"

            let result = parse cmd

            channel.send result

            cli $"command executed > {result}"
