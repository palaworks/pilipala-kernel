[<AutoOpen>]
module pilipala.builder.useService

open System.IO
open System.Reflection
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open Microsoft.Extensions.DependencyInjection
open pilipala.service

type Builder with

    member self.useService t =

        let f (sc: IServiceCollection) =
            sc
                .BuildServiceProvider()
                .GetService<ServiceProvider>()
                .regServiceByType t

            sc

        { pipeline = self.pipeline .> f }

    member self.useService<'s when 's :> ServiceAttribute>() = self.useService typeof<'s>

    /// 从程序集注册
    /// dir示例：./serv/Palang
    /// 内含dll文件：Palang.dll
    /// 在 pilipala.serv 命名空间下应具有类型 Palang
    member self.useService dir =
        let servDir = DirectoryInfo(dir)
        let servName = servDir.Name

        let servDll =
            servDir.GetFileSystemInfos().toList ()
            |> filterOnce (fun x -> x.Name = $"{servName}.dll")
            |> unwrap

        let servDllPath = servDll.FullName

        let servType =
            Assembly
                .LoadFrom(servDllPath)
                .GetType($"pilipala.serv.{servName}")

        self.useService servType
