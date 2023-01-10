[<AutoOpen>]
module pilipala.builder.usePlugin

open System
open System.IO
open System.Reflection
open System.Runtime.Loader
open fsharper.op
open fsharper.typ
open Microsoft.Extensions.DependencyInjection
open pilipala.log
open pilipala.plugin
open pilipala.util.di
open pilipala.plugin.util

type Builder with

    member self.usePlugin(t: Type) =
        let attr: HookOnAttribute =
            downcast t.GetCustomAttribute(typeof<HookOnAttribute>, true)

        let f (sc: IServiceCollection) =
            sc.UpdateSingleton<PluginRegister>(fun old -> old.registerPlugin t attr.time)

        { pipeline = self.pipeline .> effect f }

    member self.usePlugin<'p when 'p: not struct>() = self.usePlugin typeof<'p>

    /// 从程序集文件夹注册
    /// dir示例：./plugin/Llink
    /// 内含dll文件：Llink.dll
    /// 在 pilipala.plugin 命名空间下应具有类型 Llink
    member self.usePlugin dir =
        let pluginDir = DirectoryInfo(dir)
        let pluginName = pluginDir.Name

        let pluginDll =
            pluginDir.GetFileSystemInfos().toList ()
            |> find (fun x -> x.Name = $"{pluginName}.dll")
            |> unwrap

        let pluginDllPath = pluginDll.FullName

        let ctx = pluginCtx pluginDllPath

        let pluginType =
            ctx
                .LoadFromAssemblyName(AssemblyName(pluginName))
                .GetType($"pilipala.plugin.{pluginName}")

        self.usePlugin pluginType
