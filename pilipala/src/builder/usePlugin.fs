[<AutoOpen>]
module pilipala.builder.usePlugin

open System
open System.IO
open System.Reflection
open System.Text.RegularExpressions
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala
open pilipala.plugin

type Builder with

    member self.usePlugin t =
        let func _ = regPluginByType t

        self.buildPipeline.mappend (Pipe(func = func))

    member self.usePlugin<'p when 'p :> PluginAttribute>() = self.usePlugin typeof<'p>

    member self.usePlugin dir =
        let pluginDir = DirectoryInfo(dir)
        let pluginName = pluginDir.Name

        let pluginDll =
            pluginDir.GetFileSystemInfos().toList ()
            |> filterOne (fun x -> x.Name = $"{pluginName}.dll")
            |> unwrap

        let pluginDllPath = pluginDll.FullName

        let pluginType =
            Assembly
                .LoadFrom(pluginDllPath)
                .GetType($"pilipala.plugin.{pluginName}")

        self.usePlugin pluginType
