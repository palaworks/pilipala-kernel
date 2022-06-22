namespace pilipala.serv.reg

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open Microsoft.Extensions.DependencyInjection
open WebSocketer.typ
open fsharper.op
open fsharper.typ.List
open fsharper.op.Reflection
open fsharper.typ.Procedure
open fsharper.typ.Array
open fsharper.typ.Option'
open pilipala.log
open pilipala.auth.channel

(*
servPath是用于路由服务结构的文本
例如：
/serv/palang
/serv/shutdown
/serv/some_plugin/1a2b...
*)

[<AutoOpen>]
module err =
    exception UnknownServAccessLevelException //未知服务访问级别异常
    exception InvalidServConstructorException //非法服务构造异常

[<AutoOpen>]
module typ =

    /// 服务访问级别
    type ServAccessLv =
        | Everyone //无需认证
        | NeedAuth //需要认证

[<AutoOpen>]
module attr =

    /// 服务属性，仅限于修饰类
    [<AttributeUsage(AttributeTargets.Class)>]
    type ServAttribute(Path: string, EntryPoint: string, AccessLv: ServAccessLv) =
        inherit Attribute()
        member val Path = Path
        member val EntryPoint = EntryPoint
        member val AccessLv = AccessLv

[<AutoOpen>]
module fn =

    /// 已注册服务路径到服务信息的映射
    let private registeredServInfo =
        Dictionary<string, {| Type: Type
                              EntryPoint: string
                              AccessLv: ServAccessLv |}>
            ()

    /// 注册服务
    let regServByType (t: Type) =
        let attr: ServAttribute =
            downcast t.GetCustomAttributes(typeof<ServAttribute>, false).[0]

        (attr.Path,
         {| Type = t
            EntryPoint = attr.EntryPoint
            AccessLv = attr.AccessLv |})
        |> registeredServInfo.TryAdd
        |> mustTrue

    /// 注册服务
    let regServ<'s when 's :> ServAttribute> () =
        //when 's :> ServAttribute, 's obviously not struct
        regServByType typeof<'s>

    /// 获取服务信息
    let getServInfo path =
        registeredServInfo.TryGetValue path
        |> Option'.fromOkComma

    /// 匹配获取服务信息
    let matchServInfo pathRegexp =
        [ for path in registeredServInfo.Keys do
              if Regex.IsMatch(path, pathRegexp) then
                  registeredServInfo.[path] ]
