namespace pilipala.service

open System
open System.Collections.Generic
open System.Text.RegularExpressions
open fsharper.op
open fsharper.typ
open fsharper.op.Alias

(*
servPath是用于路由服务结构的文本
例如：
/serv/palang
/serv/shutdown
/serv/some_plugin/1a2b...
*)

//exception UnknownServiceAccessLevelException //未知服务访问级别异常
//exception InvalidServiceConstructorException //非法服务构造异常

/// 服务访问级别
type ServiceAccessLv =
    | Everyone //无需认证
    | NeedAuth //需要认证

/// 服务属性，仅限于修饰类
[<AttributeUsage(AttributeTargets.Class)>]
type ServiceAttribute(Path: string, EntryPoint: string, AccessLv: ServiceAccessLv) =
    inherit Attribute()
    member val Path = Path
    member val EntryPoint = EntryPoint
    member val AccessLv = AccessLv

type ServiceProvider() =

    /// 已注册服务路径到服务信息的映射
    member self.registeredServInfo =
        Dict<string, {| Type: Type
                        EntryPoint: string
                        AccessLv: ServiceAccessLv |}>
            ()


    /// 注册服务
    member self.regServiceByType(t: Type) =
        let attr: ServiceAttribute =
            downcast t.GetCustomAttributes(typeof<ServiceAttribute>, false).[0]

        (attr.Path,
         {| Type = t
            EntryPoint = attr.EntryPoint
            AccessLv = attr.AccessLv |})
        |> self.registeredServInfo.TryAdd
        |> mustTrue

    /// 注册服务
    member self.regService<'s when 's :> ServiceAttribute>() =
        //when 's :> ServAttribute, 's obviously not struct
        self.regServiceByType typeof<'s>

    /// 获取服务信息
    member self.getServiceInfo path =
        self.registeredServInfo.TryGetValue path
        |> Option'.fromOkComma

    /// 匹配获取服务信息
    member self.matchServiceInfo pathRegexp =
        [ for path in self.registeredServInfo.Keys do
              if Regex.IsMatch(path, pathRegexp) then
                  self.registeredServInfo.[path] ]
