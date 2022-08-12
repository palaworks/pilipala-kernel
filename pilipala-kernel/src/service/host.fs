module pilipala.service.host

open System
open System.Net.Sockets
open System.Threading.Tasks
open System.Security.Cryptography
open fsharper.op
open fsharper.typ
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open WebSocketer.typ
open pilipala.id
open pilipala.service
open pilipala.util.crypto
open pilipala.util.encoding
open pilipala.access.auth.token
open pilipala.access.auth.channel

type RunServiceHost = { runAsync: IServiceProvider -> Task }

/// 构造服务主机
let make (sp: IServiceProvider) =
    let serviceRegister =
        sp.GetRequiredService<ServiceRegister>()

    let uuidGenerator =
        sp.GetRequiredService<IUuidGenerator>()

    let tokenProvider =
        sp.GetRequiredService<TokenProvider>()

    let f (scopedServiceProvider: IServiceProvider) =

        let ws =
            scopedServiceProvider.GetRequiredService<WebSocket>()

        //request <service_path>
        let servicePath = ws.recv().Split(' ').[1] //服务路径

        let serviceInfo =
            serviceRegister
                .getServiceInfo(servicePath)
                .unwrapOr (fun _ -> failwith "Invalid service path")

        let serviceALv = serviceInfo.snd().AccessLv //服务访问级别
        let serviceType = serviceInfo.snd().Type

        match serviceALv with
        | Everyone ->
            let service =
                scopedServiceProvider.GetRequiredService serviceType

            //添加非加密网络信道
            let chan = NetChannel(ws)

            for response in
                Seq.unfold
                <| fun request ->
                    //从服务入口点调用服务
                    match service.tryInvoke ((snd serviceInfo).EntryPoint, [| request () |]) with
                    | None -> Option.None
                    | Some response -> Option.Some(response, chan.Read)
                <| chan.Read do
                chan.Write response //回送服务结果
        | NeedAuth ->
            ws.send "need auth" //服务端问候
            let clientPubKey = ws.recv () //接收客户公钥

            let sessionKey = uuidGenerator.next () //生成会话密钥

            sessionKey //将会话密钥使用客户公钥加密后送回
            |> rsa.encrypt clientPubKey RSAEncryptionPadding.Pkcs1
            |> ws.send

            //接收密文解密到凭据
            let clientToken =
                ws.recv ()
                |> aes.decrypt (hexToBytes sessionKey) [||] CipherMode.ECB PaddingMode.Zeros

            let whenCheckPass () =
                ws.send "auth pass" //受信通告

                let service =
                    scopedServiceProvider.GetRequiredService serviceType

                //添加加密网络信道
                let chan = NetChannel(ws, sessionKey)

                for response in
                    Seq.unfold
                    <| fun request ->
                        //从服务入口点调用服务
                        match service.tryInvoke ((snd serviceInfo).EntryPoint, [| request () |]) with
                        | None -> Option.None
                        | Some response -> Option.Some(response, chan.Read)
                    <| chan.Read do
                    chan.Write response //回送服务结果

            let whenCheckFailed () = ws.send "auth failed"

            //凭据校验
            if tokenProvider.check clientToken then
                whenCheckPass ()
            else
                whenCheckFailed () //凭据无效

        ws.Dispose()

    { new BackgroundService() with
        member self.ExecuteAsync ct =
            fun _ ->
                try
                    while not ct.IsCancellationRequested do //持续循环到主机取消
                        let scope = sp.CreateScope()
                        f scope.ServiceProvider
                with
                | :? SocketException -> ()
            |> Task.RunAsTask }
