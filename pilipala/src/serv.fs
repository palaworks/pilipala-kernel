namespace pilipala.serv

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open Microsoft.Extensions.DependencyInjection
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
    type ServAttribute(path: string, AccessLv: ServAccessLv) =
        inherit Attribute()
        member val Path = path
        member val AccessLv = AccessLv

[<AutoOpen>]
module fn =

    /// 已注册服务集合
    let private registeredServ = ServiceCollection()
    let registeredServ.BuildServiceProvider()
    //<servPath, ServAccessLv * (NetChannel -> ILog -> unit)>()

    /// 注册服务
    let regServ<'s when 's: not struct> ()=

        registeredServ.AddTransient<'s>()

    let endReg=
        registeredServ.BuildServiceProvider()
    /// 获取服务
    let getServ<'s when 's: not struct> ()=

    let matchServ servPathRegexp =
        [ for servPath in registeredServCons.Keys do
              if Regex.IsMatch(servPath, servPathRegexp) then
                  let ok, (lv, cons) =
                      registeredServCons.TryGetValue(servPath)

                  ok |> mustTrue
                  cons: Lazy<NetChannel> -> Lazy<ILog> -> unit ]
