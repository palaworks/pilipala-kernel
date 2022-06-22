[<AutoOpen>]
module pilipala.builder.useAuth

open System.Net
open System.Net.Sockets
open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open WebSocketer.typ
open pilipala.builder
open pilipala.serv.host

//TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

type Builder with

    member self.useAuth(port: u16) =
        let server = //用于监听的服务器
            TcpListener(IPAddress.Parse("localhost"), i32 port)

        server.Start()

        let host =
            Host
                .CreateDefaultBuilder()
                .ConfigureServices(fun services ->
                    services
                        //添加WS
                        .AddScoped<WebSocket>(fun _ ->
                            server.AcceptTcpClient()
                            |> fun c -> new WebSocket(c))
                        //添加服务主机
                        .AddHostedService<ServHost>()
                    |> ignore)
                .Build()

        self.buildPipeline.mappend (Pipe(func = host.Run))
