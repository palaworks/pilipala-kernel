namespace pilipala.service

open System
open System.IO
open System.Collections.Generic
open fsharper.op
open fsharper.op.Reflection
open fsharper.types.Array
open fsharper.types.Option'
open pilipala.auth.channel
open pilipala.util.encoding

[<AutoOpen>]
module err =
    exception UnknownServAccessLevelException //未知服务访问级别异常
    exception InvalidServConstructorException //非法服务构造异常

[<AutoOpen>]
module types =
    /// 服务日志
    type ServLog(s: Stream) =
        member self.log text = text |> utf8ToBytes |> s.Write

    /// 服务类型
    type ServType =
        | Pub //无需认证
        | Pri //需要认证

[<AutoOpen>]
module attr =

    /// 服务属性，仅限于修饰类
    [<AttributeUsage(AttributeTargets.Class)>]
    type ServAttribute(Name: string) =
        inherit Attribute()
        member val Name = Name

    /// 公共服务属性，仅限于修饰类
    [<AttributeUsage(AttributeTargets.Class)>]
    type PriServAttribute() =
        inherit Attribute()

    /// 私有服务属性，仅限于修饰类
    [<AttributeUsage(AttributeTargets.Class)>]
    type PubServAttribute() =
        inherit Attribute()

[<AutoOpen>]
module fn =

    /// 服务集
    let private services =
        Dictionary<string, ServType * (ServChannel -> unit)>()

    let inline isPubServ<'s> =
        typeof<'s>.GetCustomAttributes(
            typeof<PubServAttribute>,
            false
        )
            .Length > 0

    let inline isPriServ<'s> =
        typeof<'s>.GetCustomAttributes(
            typeof<PriServAttribute>,
            false
        )
            .Length > 0

    let inline getServAttribute<'s> : ServAttribute =
        typeof<'s>.GetCustomAttributes (typeof<ServAttribute>, false)
        |> get 0u
        |> coerce

    let regService<'s> logStream =
        let servName = getServAttribute<'s>.Name

        let accessLv = //服务访问级别
            if isPubServ<'s> then
                Pub
            elif isPriServ<'s> then
                Pri
            else
                raise UnknownServAccessLevelException

        let handler chan =
            let servInstance =
                //只允许按照 ServLog ServChannel 的顺序构造
                match typeof<'s>.GetConstructor [| typeof<ServLog>; typeof<ServChannel> |] with
                | null ->
                    match typeof<'s>.GetConstructor [| typeof<ServChannel> |] with
                    | null -> raise InvalidServConstructorException
                    | c -> c.Invoke [| chan |]
                | c -> c.Invoke([| ServLog logStream, chan |])

            servInstance.tryInvoke "start" //启动服务

            logStream.Close() //服务结束，关闭日志流

        services.Add(servName, (accessLv, handler))

    let getService servName =
        let exist, h = services.TryGetValue servName
        if exist then Some h else None
