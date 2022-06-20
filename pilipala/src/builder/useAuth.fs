[<AutoOpen>]
module pilipala.builder.useAuth

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading.Tasks
open System.Security.Cryptography
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open WebSocketer.typ
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Coerce
open fsharper.op.Reflection
open fsharper.typ.Pipe.Pipable
open pilipala.builder
open pilipala.log
open pilipala.serv
open pilipala.util.uuid
open pilipala.auth.token
open pilipala.util.crypto
open pilipala.auth.channel
open pilipala.util.encoding

//TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

let log (text: string) =
    Console.WriteLine $"pilipala auth service : {text}"

(*
        | :? SocketException -> log "connection lost"
        | _ -> log "auth failed"


    /// 使用认证
        let whenNeedAuthDo (ws: WebSocket) handler =
            ws.send "need auth" //服务端问候
            log "start authing"

            let pubKey = ws.recv () //接收客户公钥
            log "pubKey received"

            let sessionKey = gen N //生成会话密钥

            //将会话密钥使用客户公钥加密后送回
            sessionKey
            |> rsa.encrypt pubKey RSAEncryptionPadding.Pkcs1
            |> ws.send

            log "sessionKey sent"

            //接收密文解密到凭据
            let token =
                ws.recv ()
                |> aes.decrypt (hexToBytes sessionKey) [||] CipherMode.ECB PaddingMode.Zeros

            log "token received"

            let whenCheckPass () =
                ws.send "pass" //受信通告
                log "client certified"

                lazy (NetChannel(ws, sessionKey)) |> handler

            let whenCheckFailed () = log "token invalid"

            //凭据校验
            if check token then
                whenCheckPass ()
            else
                whenCheckFailed () //凭据无效

        let whenEveryoneDo ws handler = ws |> PubChannel |> handler //公共服务，无需验证
*)

// 对绘画密钥的引用包装
//type SessionKey = { value: string }
// 对验证端口的引用包装
//type AuthPort = { value: u16 }

/// 用于验证的服务主机
type private HostService(scopeFac: IServiceScopeFactory) =
    inherit BackgroundService()
    with
        override self.ExecuteAsync ct =
            fun _ ->
                while not ct.IsCancellationRequested do //持续循环到主机取消

                    let servScope = scopeFac.CreateScope()
                    let servProvider = servScope.ServiceProvider

                    //request <serv_path>
                    let ws = servProvider.GetService<WebSocket>()
                    let servPath = ws.recv().Split(' ').[1] //服务路径

                    let serv = //通过服务路径获取服务
                        servProvider.GetService registeredServPath.[servPath]

                    let servAttr: ServAttribute = coerce serv //服务特性
                    let servALv = servAttr.AccessLv //服务访问级别

                    serv.tryInvoke servAttr.EntryPoint //从服务入口点启动服务

            |> Task.RunAsTask


type palaBuilder with

    member self.useAuth(port: u16) =
        let server = //用于监听的服务器
            TcpListener(IPAddress.Parse("localhost"), i32 port)

        server.Start()

        let host =
            Host
                .CreateDefaultBuilder()
                .ConfigureServices(fun _ services ->
                    //添加已注册服务
                    for s in registeredServ do
                        services.Add s

                    services
                        //添加WS
                        .AddScoped<WebSocket>(fun _->
                            server.AcceptTcpClient()
                            |>fun c->new WebSocket(c)
                            )
                        //添加不安全网络信道
                        .AddScoped<UnsafeNetChannel>(fun _ ->
                            server.AcceptTcpClient()
                            |> fun c -> new WebSocket(c)
                            |> UnsafeNetChannel)
                        //添加安全网络信道
                        .AddScoped<SafeNetChannel>(fun _ ->
                            server.AcceptTcpClient()
                            |> fun c -> new WebSocket(c)
                            |> fun ws -> SafeNetChannel(ws, gen N))
                        //添加服务主机
                        .AddHostedService<HostService>()
                    |> ignore)
                .Build()

        self.buildPipeline.mappend (Pipe(func = host.Run))
