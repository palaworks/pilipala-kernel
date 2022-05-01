module pilipala.palang.service

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
open pilipala.palang.analyse

let palangService (channel: SecureChannel) =
    let log msg =
        Console.WriteLine $"palang service : {msg}"

    log "online"

    while true do //持续执行命令
        let cmd = channel.recvMsg ()
        log $"command received < {cmd}"

        let result = analyse_cmd cmd

        channel.sendMsg result
        log $"command executed > {result}"
