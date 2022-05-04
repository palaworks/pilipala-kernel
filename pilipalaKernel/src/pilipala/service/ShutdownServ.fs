namespace pilipala.service

open System
open fsharper.op.Fmt
open pilipala.auth.channel
open pilipala.palang.analyse
open pilipala.service

/// 关闭噼里啪啦服务
[<PriServ>]
[<Serv("shutdown")>]
type ShutdownServ() =
    member self.start() =
        //TODO 等待实现
        ()
