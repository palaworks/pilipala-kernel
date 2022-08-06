namespace pilipala.service

open System.Net.Sockets
open System.Threading.Tasks
open System.Security.Cryptography
open fsharper.typ
open fsharper.op.Boxing
open fsharper.op.Reflect
open WebSocketer.typ
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open pilipala.id
open pilipala.log
open pilipala.util.crypto
open pilipala.util.encoding
open pilipala.access.auth.token
open pilipala.access.auth.channel

/// 服务执行主机
type internal ServiceHost
    (
        scopeFac: IServiceScopeFactory,
        sp: ServiceRegister,
        lp: LogRegister,
        tp: TokenProvider,
        uuid: IUuidGenerator
    ) =
    inherit BackgroundService()

    let func _ =

        let servScope = scopeFac.CreateScope()
        let servProvider = servScope.ServiceProvider

        //request <serv_path>
        let ws =
            servProvider.GetService<WebSocket>()

        let servPath = ws.recv().Split(' ').[1] //服务路径

        let servInfo =
            sp.getServiceInfo servPath |> unwrap

        let servALv = servInfo.AccessLv //服务访问级别
        let servType = servInfo.Type

        let sc =
            ServiceCollection()
                .AddTransient(servType)
                .AddLogging(fun builder ->
                    for kv in lp.registeredLoggerFilter do
                        (kv.Key, kv.Value) |> builder.AddFilter |> ignore

                    for p in lp.registeredLoggerProvider do
                        p |> builder.AddProvider |> ignore)

        match servALv with
        | Everyone ->
            sc.AddScoped<_>(fun _ -> NetChannel(ws)) |> ignore

            let serv =
                sc.BuildServiceProvider().GetService servType

            serv.tryInvoke servInfo.EntryPoint //从服务入口点启动服务

        | NeedAuth ->
            ws.send "need auth" //服务端问候
            let pubKey = ws.recv () //接收客户公钥

            let sessionKey = uuid.next () //生成会话密钥

            sessionKey //将会话密钥使用客户公钥加密后送回
            |> rsa.encrypt pubKey RSAEncryptionPadding.Pkcs1
            |> ws.send

            //接收密文解密到凭据
            let token =
                ws.recv ()
                |> aes.decrypt (hexToBytes sessionKey) [||] CipherMode.ECB PaddingMode.Zeros

            let whenCheckPass () =
                ws.send "auth pass" //受信通告

                //添加安全网络信道
                sc.AddScoped<_>(fun _ -> NetChannel(ws, sessionKey))
                |> ignore

                let serv =
                    sc.BuildServiceProvider().GetService servType

                serv.tryInvoke servInfo.EntryPoint //从服务入口点启动服务

            let whenCheckFailed () = ws.send "auth failed"

            //凭据校验
            if tp.check token then
                whenCheckPass ()
            else
                whenCheckFailed () //凭据无效

        ws.Dispose()
    with

        override self.ExecuteAsync ct =
            fun _ ->
                try
                    while not ct.IsCancellationRequested do //持续循环到主机取消
                        func ()
                with
                | :? SocketException -> ()
            |> Task.RunAsTask
