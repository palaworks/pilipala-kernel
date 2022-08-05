namespace pilipala.plugin

open System
open fsharper.op.Pattern
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open pilipala.log

(*
插件需要遵循下列规范：
插件根目录：
./pilipala/plugin
插件文件夹名和插件dll名和插件名需一致：
./pilipala/plugin/Llink/Llink.dll
插件的启动应由其构造函数完成，这与服务所谓的入口点不同。
*)

//虽然理论上该实现能够启动同一文件夹下的众多插件（dll），但建议的实践是一个插件（dll）放一个文件夹

/// 插件特性，仅限修饰类
[<AttributeUsage(AttributeTargets.Class)>]
type PluginAttribute() =
    inherit Attribute()

//dir目录下应有多个文件夹
//每个文件夹对应一个插件，例如：
//./pilipala/plugin/Llink
//./pilipala/plugin/Palang
//./pilipala/plugin/Mailssage

type internal PluginDispatcher(sc: IServiceCollection, lp: LogRegister) =

    /// 启动插件
    member self.launchPluginByType t =
        //每次都需要重新构建DI容器，因为每次插件的执行都可能带来新的依赖
        let scopedSC: IServiceCollection =
            ServiceCollection()

        for sd in sc do
            scopedSC.Add sd

        scopedSC
            .AddLogging(fun builder ->
                for KV (k, v) in lp.registeredLoggerFilter do
                    builder.AddFilter(k, v) |> ignore

                for p in lp.registeredLoggerProvider do
                    builder.AddProvider p |> ignore)
            .AddTransient(t)
            .BuildServiceProvider()
            .GetService(t)
        |> ignore

    /// 启动插件
    member self.launchPlugin<'p when 'p :> PluginAttribute>() =
        //when 'p :> PluginAttribute, 'p obviously not struct
        self.launchPluginByType typeof<'p>
