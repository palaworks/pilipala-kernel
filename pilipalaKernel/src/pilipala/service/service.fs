module pilipala.service

open System.Collections.Generic
open pilipala.auth.channel

/// 私有服务集
let private priServices = Dictionary<string, PriChannel -> unit>()
/// 公有服务集
let private pubServices = Dictionary<string, PubChannel -> unit>()

let registService serviceName (serviceHandler: ServChannel -> unit) =
    match serviceHandler with
    | :? (PubChannel -> unit) -> pubServices.Add(serviceName, serviceHandler)
    | :? (PubChannel -> unit) -> priServices.Add(serviceName, serviceHandler)
