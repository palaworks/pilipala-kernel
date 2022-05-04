namespace pilipala.service

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse
open pilipala.service

/// palang语言服务
[<PriServ>]//TODO 能否强制多特性组合？
[<Serv("palang")>]
type PalangServ(sl: ServLog, chan: ServChannel) =
    member self.start() =
        
        while true do //持续执行命令
            let cmd = chan.recvMsg ()
            sl.log $"command received < {cmd}"

            let result = analyse_cmd cmd

            chan.sendMsg result
            sl.log $"command executed > {result}"
