namespace pilipala.service

open System
open System.IO
open System.Collections.Generic
open fsharper.op
open fsharper.op.Reflection
open fsharper.typ.Array
open fsharper.typ.Option'
open pilipala.auth.channel
open pilipala.util.encoding

[<AutoOpen>]
module err =
    exception UnknownServAccessLevelException //未知服务访问级别异常
    exception InvalidServConstructorException //非法服务构造异常

[<AutoOpen>]
module typ =
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

    let inline isPubServ<'s> = hasAttr<'s, PubServAttribute>

    let inline isPriServ<'s> = hasAttr<'s, PriServAttribute>

    let inline getServAttribute<'s> : ServAttribute =
        typeof<'s>.GetCustomAttributes (typeof<ServAttribute>, false)
        |> get 0u
        |> coerce

    let regService<'s> logStreamGetter =
        let servName = getServAttribute<'s>.Name

        let accessLv = //服务访问级别
            if isPubServ<'s> then
                Pub
            elif isPriServ<'s> then
                Pri
            else
                raise UnknownServAccessLevelException

        //将反射式依赖注入的分析开销转移至服务注册阶段
        let runServ =
            let startMethod = typeof<'s>.GetMethod "start"

            //此实现只允许按照 ServChannel ServLog 的顺序构造

            //什么都不用的触发式服务
            let case4 () =
                match typeof<'s>.GetConstructor [||] with
                | null -> raise InvalidServConstructorException
                | c -> fun _ _ -> c.Invoke([||])
            //仅使用日志
            let case3 () =
                match typeof<'s>.GetConstructor [| typeof<ServLog> |] with
                | null -> case4 ()
                | c -> fun _ log -> startMethod.Invoke(c.Invoke [| force log |], [||])
            //仅使用通道
            let case2 () =
                match typeof<'s>.GetConstructor [| typeof<ServChannel> |] with
                | null -> case3 ()
                | c -> fun chan _ -> startMethod.Invoke(c.Invoke [| force chan |], [||])
            //同时使用通道和日志
            let case1 () =
                match typeof<'s>.GetConstructor [| typeof<ServChannel>; typeof<ServLog> |] with
                | null -> case2 ()
                | c -> fun chan log -> startMethod.Invoke(c.Invoke [| force chan; force log |], [||])

            case1 ()

        let handler chan =
            use s = logStreamGetter ()
            runServ (lazy (chan)) (lazy (ServLog s)) |> ignore

        services.Add(servName, (accessLv, handler))

    let getService servName =
        services.TryGetValue servName
        |> Option'.fromOkComma
