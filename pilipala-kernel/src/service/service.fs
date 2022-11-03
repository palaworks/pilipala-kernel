//TODO service is deprecated

namespace pilipala.service

open System
open System.Text.RegularExpressions
open fsharper.typ

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

type ServiceRegister =
    // 已注册服务路径到服务信息的映射
    { ServiceInfos: (string * {| Type: Type
                                 EntryPoint: string
                                 AccessLv: ServiceAccessLv |}) list }


//最终整合时应使用foldr以保证顺序
type ServiceRegister with

    /// 注册服务
    member self.registerService(t: Type) =
        let attr: ServiceAttribute =
            downcast t.GetCustomAttributes(typeof<ServiceAttribute>, false).[0]

        let info =
            attr.Path,
            {| Type = t
               EntryPoint = attr.EntryPoint
               AccessLv = attr.AccessLv |}

        { ServiceInfos = info :: self.ServiceInfos }

    /// 注册服务
    member self.registerService<'s when 's :> ServiceAttribute>() =
        //when 's :> ServAttribute, 's obviously not struct
        self.registerService typeof<'s>

    /// 获取服务信息
    member self.getServiceInfo path =
        self.ServiceInfos
        |> find (fun x -> fst x = path)

    /// 匹配获取服务信息
    member self.matchServiceInfo pathRegexp =
        [ for path, info in self.ServiceInfos do
              if Regex.IsMatch(path, pathRegexp) then
                  info ]
