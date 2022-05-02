module pilipala.service.versionServ

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse

/// 版本信息服务
let versionServ (sl: ServLog, chan: ServChannel) = sl.log "v1.0.0" |> chan.sendMsg
