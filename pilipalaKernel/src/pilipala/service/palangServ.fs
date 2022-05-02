module pilipala.service.palang

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse

/// palang语言服务
let palangServ (sl: ServLog, chan: ServChannel) =

    while true do //持续执行命令
        let cmd = chan.recvMsg ()
        sl.log $"command received < {cmd}" |> ignore

        let result = analyse_cmd cmd

        chan.sendMsg result
        sl.log $"command executed > {result}" |> ignore
