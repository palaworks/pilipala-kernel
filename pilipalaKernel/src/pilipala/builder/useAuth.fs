[<AutoOpen>]
module pilipala.builder.useAuth

open System.IO
open System.Threading.Tasks
open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala.service
open System.Net.Sockets
open System.Security.Cryptography
open WebSocketer.Type
open pilipala.log
open pilipala.util.uuid
open pilipala.auth.token
open pilipala.util.crypto
open pilipala.auth.channel
open pilipala.util.encoding

type palaBuilder with

    /// 使用认证
    member self.useAuth port =
        use sw = new StreamWriter(genLogStream ())

        let log (text: string) =
            sw.WriteLine $"pilipala auth service : {text}"

        let whenPriServDo (ws: WebSocket) handler =
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

            //TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性
            //接收密文解密到凭据
            let token =
                ws.recv ()
                |> aes.decrypt (hexToBytes (sessionKey)) [||] CipherMode.ECB PaddingMode.Zeros

            log "token received"

            let whenCheckPass () =
                ws.send "pass" //受信通告
                log "client certified"

                PriChannel(ws, sessionKey) |> handler

            let whenCheckFailed () = log "token invalid"

            //凭据校验
            match check token with
            | Ok true -> whenCheckPass ()
            | _ -> whenCheckFailed () //凭据无效

        let whenPubServDo ws handler = ws |> PubChannel |> handler //公共服务，无需验证

        let func _ =
            fun _ ->
                fun _ ->
                    try
                        log "auth online"
                        let ws = new WebSocket(port) //阻塞
                        log "\nnew client connected"

                        let cmd = ws.recv ()
                        let serv_name = cmd.Split(' ').[1]

                        match getService serv_name with
                        | Some (Pri, handler) -> whenPriServDo ws handler
                        | Some (Pub, handler) -> whenPubServDo ws handler
                        | None -> log "serv rejected" //拒绝服务
                    with
                    | :? SocketException -> log "connection lost"
                    | _ -> log "auth failed"
                |> loop
            |> Task.Run
            |> ignore

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
