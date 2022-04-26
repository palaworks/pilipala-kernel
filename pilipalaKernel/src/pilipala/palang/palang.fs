module pilipala.palang

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

//TODO 有待减少样板代码
/// 各类命令解析

/// 未知语法错误
exception UnknownSyntax

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
           | 6, "bind" -> bind_parse
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
