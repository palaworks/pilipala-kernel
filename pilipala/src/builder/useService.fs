[<AutoOpen>]
module pilipala.builder.useService

open System.IO
open System.Net
open System.Reflection
open System.Net.Sockets
open fsharper.op
open fsharper.typ
open fsharper.alias
open WebSocketer.typ
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open pilipala.service
open pilipala.util.di

type Builder with

    member self.useService t =

        let f (sc: IServiceCollection) =
            sc.UpdateSingleton<ServiceRegister>(fun old -> old.registerService t)

        { pipeline = self.pipeline .> effect f }

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

    /// 在指定端口启动服务主机
    member self.ServeOn(port: u16) =

        let f _ =
            let server = //用于监听的服务器
                TcpListener(IPAddress.Parse("localhost"), i32 port)

            server.Start()

            Host
                .CreateDefaultBuilder()
                .ConfigureServices(fun services ->
                    services
                        //添加WS
                        .AddScoped<WebSocket>(fun _ ->
                            server.AcceptTcpClient()
                            |> fun c -> new WebSocket(c))
                        //添加服务主机
                        .AddHostedService<ServiceHost>()
                    |> ignore)
                .Build()
                .Run()

        { pipeline = self.pipeline .> effect f }
