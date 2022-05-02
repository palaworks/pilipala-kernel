module pilipala.service.palang

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse

/// palang语言服务
let palangServ (chan: PriChannel) =
    let log msg = println $"palang service : {msg}"

    log "online"

    while true do //持续执行命令
        let cmd = chan.recvMsg ()
        log $"command received < {cmd}"

        let result = analyse_cmd cmd

        chan.sendMsg result
        log $"command executed > {result}"
