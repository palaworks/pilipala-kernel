[<AutoOpen>]
module pilipala.builder.usePlugin

open System.IO
open System.Reflection
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open Microsoft.Extensions.DependencyInjection
open pilipala.plugin

type Builder with

    member self.usePlugin t =
        let f (sc: IServiceCollection) =
            sc
                .BuildServiceProvider()
                .GetService<PluginProvider>()
                .launchPluginByType t

            sc

        { pipeline = self.pipeline.export (StatePipe(activate = f)) }

    member self.usePlugin<'p when 'p :> PluginAttribute>() = self.usePlugin typeof<'p>

    /// 从程序集文件夹注册
    /// dir示例：./plugin/Llink
    /// 内含dll文件：Llink.dll
    /// 在 pilipala.plugin 命名空间下应具有类型 Llink
    member self.usePlugin dir =
        let pluginDir = DirectoryInfo(dir)
        let pluginName = pluginDir.Name

        let pluginDll =
            pluginDir.GetFileSystemInfos().toList ()
            |> filterOnce (fun x -> x.Name = $"{pluginName}.dll")
            |> unwrap

        let pluginDllPath = pluginDll.FullName

        let pluginType =
            Assembly
                .LoadFrom(pluginDllPath)
                .GetType($"pilipala.plugin.{pluginName}")

        self.usePlugin pluginType
