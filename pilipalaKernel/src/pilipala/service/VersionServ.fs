namespace pilipala.service

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse
open pilipala.service

/// 版本信息服务
[<PubServ>]
[<Serv("version")>]
type VersionServ(sl: ServLog, chan: ServChannel) =
    member self.start() =
        
        sl.log "v1.0.0"
        chan.sendMsg "v1.0.0"
