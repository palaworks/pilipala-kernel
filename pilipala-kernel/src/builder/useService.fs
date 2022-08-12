[<AutoOpen>]
module pilipala.builder.useService

open System
open System.IO
open System.Net
open System.Reflection
open System.Net.Sockets
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Foldable
open WebSocketer.typ
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open pilipala.id
open pilipala.log
open pilipala.builder
open pilipala.service
open pilipala.util.di
open pilipala.access.user
open pilipala.service.host
open pilipala.container.post
open pilipala.container.comment

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


type Builder with

    /// 设置服务主机启动端口
    member self.serveOn(port: u16) =

        //sp为已构建的DI容器
        let runAsync (sp: IServiceProvider) =
            Host
                .CreateDefaultBuilder()
                .ConfigureServices(fun sc ->

                    let server = //用于监听的服务器
                        TcpListener(IPAddress.Parse("localhost"), i32 port)
                        |> effect (fun listener -> listener.Start())

                    sp
                        .GetService<ServiceRegister>()
                        .ServiceInfos
                        .foldr
                    //每作用域注入服务
                    <| fun (_, info) (acc: IServiceCollection) -> acc.AddScoped info.Type
                    <| sc
                    |> ignore

                    sc
                        //为服务主机配置相同的日志
                        .AddLogging(fun builder ->
                            let lr = sp.GetService<LoggerRegister>()

                            lr.LoggerFilters.foldr
                            <| (fun (k, v) (acc: ILoggingBuilder) -> acc.AddFilter(k, v))
                            <| builder
                            |> ignore

                            lr.LoggerProviders.foldr
                            <| (fun p (acc: ILoggingBuilder) -> acc.AddProvider p)
                            <| builder
                            |> ignore)
                        //由于构建已完成，所以此处的设施全为单例注入
                        .AddSingleton<IMappedPostProvider>(fun _ -> sp.GetService<_>())
                        .AddSingleton<IMappedCommentProvider>(fun _ -> sp.GetService<_>())
                        .AddSingleton<IMappedUserProvider>(fun _ -> sp.GetService<_>())
                        .AddSingleton<IPalaflakeGenerator>(fun _ -> sp.GetService<_>())
                        .AddSingleton<IUuidGenerator>(fun _ -> sp.GetService<_>())
                        .AddSingleton<ServiceRegister>(fun _ -> sp.GetService<_>())
                        .AddScoped<WebSocket>(fun _ -> new WebSocket(server.AcceptTcpClient()))
                        .AddHostedService(fun sp' -> host.make sp')
                    |> ignore)
                .Build()
                .RunAsync()

        let f (sc: IServiceCollection) =
            sc.UpdateSingleton<RunServiceHost>(fun _ -> { runAsync = runAsync })

        { pipeline = self.pipeline .> f }
