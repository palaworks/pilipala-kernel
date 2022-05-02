[<AutoOpen>]
module palaKernel.pilipala.auth.fn

open System
open System.Net.Sockets
open System.Security.Cryptography
open WebSocketer.Type
open fsharper.types
open fsharper.op.Fmt
open fsharper.types.Pipe.Pipable
open pilipala
open pilipala.service
open pilipala.util.uuid
open pilipala.auth.token
open pilipala.util.crypto
open pilipala.auth.channel

let private cli (msg: string) =
    Console.WriteLine $"pilipala auth service : {msg}"

let serveOn port =

    async {
        cli "auth online"

        let ws = new WebSocket(port) //阻塞

        cli "\nnew client connected"

        try
            let serv_cmd = ws.recv().Split(' ')
            let serv_name = serv_cmd.[1]

            match getService serv_name with
            | Some (Pri, handler) ->
                ws.send "need auth" //服务端问候
                cli "start authing"

                let pubKey = ws.recv () //接收客户公钥
                cli "pubKey received"

                let sessionKey = gen N //生成会话密钥

                //将会话密钥使用客户公钥加密后送回
                sessionKey
                |> rsa.encrypt pubKey RSAEncryptionPadding.Pkcs1
                |> ws.send

                cli "sessionKey sent"

                //TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性
                //接收密文解密到凭据
                let token =
                    ws.recv ()
                    |> aes.decrypt (Convert.FromHexString(sessionKey)) [||] CipherMode.ECB PaddingMode.Zeros

                cli "token received"

                //凭据校验
                match check token with
                | Ok true ->
                    ws.send "pass" //受信通告
                    cli "client certified"

                    PriChannel(ws, sessionKey) |> handler

                | _ -> cli "token invalid" //凭据无效
            | Some (Pub, handler) -> ws |> PubChannel |> handler //公共服务，无需验证
            | None -> cli "serv rejected" //拒绝服务
        with
        | :? SocketException -> cli "connection lost"
        | _ -> cli "auth failed"
        |> ignore
    }
    |> Async.Start