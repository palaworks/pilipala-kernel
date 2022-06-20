namespace pilipala.serv

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

type private servPath = string

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

    /// 已注册服务集合
    let internal registeredServ = ServiceCollection()

    /// 已注册服务路径到服务的映射
    let internal registeredServPath = Dictionary<string, Type>()

    /// 注册服务
    let regServ<'s when 's :> ServAttribute and 's: not struct> =
        let t = typeof<'s>

        let attr: ServAttribute =
            downcast t.GetCustomAttributes(typeof<ServAttribute>, false).[0]

        registeredServPath.TryAdd(attr.Path, t)
        |> mustTrue

        registeredServ.AddScoped<'s>() |> ignore

(*
    /// 获取服务
    let getServ<'s when 's :> ServAttribute and 's: equality and 's: null> () =
        registeredServ
            .BuildServiceProvider()
            .GetService<'s>()
        |> Option'.fromNullable

    /// 通过服务路径获取服务
    let getServByPath<'s when 's :> ServAttribute> path : Option'<'s> =
        let serv =
            registeredServPath.[path]
            |> registeredServ.BuildServiceProvider().GetService

        if serv = null then
            None
        else
            serv |> coerce |> Some

    /// 匹配服务路径获取服务
    let matchServ pathRegexp : ServAttribute list =
        [ for path in registeredServPath.Keys do
              if Regex.IsMatch(path, pathRegexp) then
                  registeredServPath.[path]
                  |> registeredServ.BuildServiceProvider().GetService
                  |> coerce ]
    *)
