module pilipala.service.versionServ

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse

/// 版本信息服务
let versionServ (chan: PubChannel) =
    let log msg = println $"version service : {msg}"

    log "online"

    while true do //持续执行命令
        let cmd = chan.recvMsg ()
        log $"command received < {cmd}"

        let result = analyse_cmd cmd

        chan.sendMsg result
        log $"command executed > {result}"
