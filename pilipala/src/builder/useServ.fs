[<AutoOpen>]
module pilipala.builder.useServ

open System.IO
open System.Reflection
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open Microsoft.Extensions.DependencyInjection
open pilipala.serv

type Builder with

    member self.useServ t =

        let f (sc: IServiceCollection) =
            sc
                .BuildServiceProvider()
                .GetService<ServProvider>()
                .regServByType t

            sc

        { pipeline = self.pipeline .> f }

    member self.useServ<'s when 's :> ServAttribute>() = self.useServ typeof<'s>

    /// 从程序集注册
    /// dir示例：./serv/Palang
    /// 内含dll文件：Palang.dll
    /// 在 pilipala.serv 命名空间下应具有类型 Palang
    member self.useServ dir =
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

        self.useServ servType
