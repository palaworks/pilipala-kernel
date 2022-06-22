namespace pilipala.plugin

open System
open System.IO
open System.Reflection
open System.Text.RegularExpressions
open fsharper.op
open fsharper.typ

//虽然理论上该实现能够启动同一文件夹下的众多插件（dll），但建议的实践是一个插件（dll）放一个文件夹
module typ =
    /// 插件特性
    type PluginAttribute()=
        inherit Attribute()

module fn =
    
    let getPluginInfos dir =
        DirectoryInfo(dir).GetFileSystemInfos().toList ()

    let rec getPluginFullNames (infos: FileSystemInfo list) =
        infos
        |> filter (fun x -> x.Extension = ".dll")
        |> foldMap (fun x -> List' [ x.FullName ])
        |> unwrap

    let rec getPluginNameAndAsms (fullNames: string list) =
        let getName fullName =
            Regex("/([^/]+?).dll").Match(fullName).Groups.[1]
                .Value

        fullNames
        |> foldMap (fun fullName -> List' [ (getName fullName, Assembly.LoadFrom fullName) ])
        |> unwrap

    let rec getPluginTypes (nameAndAsms: (string * Assembly) list) =
        nameAndAsms
        |> foldMap (fun (name, asm: Assembly) -> List' [ asm.GetType($"pilipala.plugin.{name}") ])
        |> unwrap

    let rec getPluginInstances (types: Type list) =
        types
        |> foldMap (fun x -> List' [ Activator.CreateInstance x ])
        |> unwrap

    let rec invokePluginInstances (instances: obj list) =
        instances
        |> map
            (fun x ->
                x.tryInvoke "inject"
                ())

    let invokePlugin path =
        path
        |> getPluginInfos
        |> getPluginFullNames
        |> getPluginNameAndAsms
        |> getPluginTypes
        |> getPluginInstances
        |> invokePluginInstances
        |> ignore
