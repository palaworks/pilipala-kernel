[<AutoOpen>]
module pilipala.service.fn

open System.Collections.Generic
open fsharper.types.Option'
open fsharper.op.Fmt
open pilipala.auth.channel

/// 服务日志
type ServLog(servName) =
    member self.log text =
        println $"{servName} service : {text}"
        text

/// 服务类型
type ServType =
    | Pub
    | Pri

/// 服务集
let private services =
    Dictionary<string, ServType * (ServChannel -> unit)>()

let regService servName servType (servHandler: ServLog -> ServChannel -> unit) =
    let handler =
        let sl = ServLog servName
        servHandler sl

    services.Add(servName, (servType, handler))

let getService servName =
    let exist, h = services.TryGetValue servName
    if exist then Some h else None
