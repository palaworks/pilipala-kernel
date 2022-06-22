namespace pilipala.plugin

open System
open System.Collections.Generic
open Microsoft.Extensions.DependencyInjection

(*
插件需要遵循下列规范：
插件根目录：
./pilipala/plugin
插件文件夹名和插件dll名和插件名需一致：
./pilipala/plugin/Llink/Llink.dll
插件的启动应由其构造函数完成，这与服务所谓的入口点不同。
*)

//虽然理论上该实现能够启动同一文件夹下的众多插件（dll），但建议的实践是一个插件（dll）放一个文件夹
[<AutoOpen>]
module typ =
    /// 插件特性，仅限修饰类
    [<AttributeUsage(AttributeTargets.Class)>]
    type PluginAttribute() =
        inherit Attribute()

[<AutoOpen>]
module fn =

    let registeredPluginInfo = List<Type>()

    //dir目录下应有多个文件夹
    //每个文件夹对应一个插件，例如：
    //./pilipala/plugin/Llink
    //./pilipala/plugin/Palang
    //./pilipala/plugin/Mailssage

    /// 注册插件
    let regPluginByType t = registeredPluginInfo.Add(t)

    /// 注册插件
    let regPlugin<'p when 'p :> PluginAttribute> () =
        //when 'p :> PluginAttribute, 'p obviously not struct
        regPluginByType typeof<'p>

    /// 启动插件
    let launchPluginByType t =
        let sc = ServiceCollection()

        for p in registeredPluginInfo do
            sc.AddTransient p |> ignore

        let provider = sc.BuildServiceProvider()

        provider.GetService t |> ignore

    /// 启动插件
    let launchPlugin<'p> () = launchPluginByType typeof<'p>

    /// 启动所有插件
    let launchAllPlugin () =
        let sc = ServiceCollection()

        for p in registeredPluginInfo do
            sc.AddTransient p |> ignore

        let provider = sc.BuildServiceProvider()

        for t in registeredPluginInfo do
            provider.GetService t |> ignore
