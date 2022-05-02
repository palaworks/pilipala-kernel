module internal pilipala.auth.serv

open System
open System.Net.Sockets
open System.Security.Cryptography
open System.Threading.Tasks
open WebSocketer.Type.WebSocket
open fsharper.types
open fsharper.op.Fmt
open pilipala.util.crypto
open pilipala.util.socket.tcp
open pilipala.util.uuid
open pilipala.auth.token
open pilipala.auth.channel

let private cli (msg: string) =
    Console.WriteLine $"pilipala auth service : {msg}"

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f
let serveOn port f =

    async {
        cli "service online"

        let ws = new WebSocket(port) //阻塞

        println ""
        cli "new client connected"

        try
            match ws.recv () with
            | "hello" -> //客户端问候
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

                    PriChannel(ws, sessionKey) |> f

                | _ -> cli "token invalid" //凭据无效
            | _ -> cli "client rejected" //非login请求
        with
        | :? SocketException -> cli "connection lost"
        | _ -> cli "auth failed"
        |> ignore
    }
    |> Async.Start
