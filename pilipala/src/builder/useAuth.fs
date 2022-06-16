[<AutoOpen>]
module pilipala.builder.useAuth

open System.IO
open System.Net.Sockets
open System.Threading.Tasks
open System.Security.Cryptography
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open WebSocketer.Type
open fsharper.typ
open fsharper.op.Coerce
open fsharper.typ.Pipe.Pipable
open pilipala.log
open pilipala.serv
open pilipala.util.uuid
open pilipala.auth.token
open pilipala.util.crypto
open pilipala.auth.channel
open pilipala.util.encoding

//TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

type palaBuilder with

    /// 使用认证
    member self.useAuth port =
        use sw = new StreamWriter(genLogStream ())

        let log (text: string) =
            sw.WriteLine $"pilipala auth service : {text}"

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

        let func _ =
            fun _ ->
                while true do
                    try
                        log "auth online"
                        let ws = new WebSocket(port) //阻塞
                        log "\nnew client connected"

                        let cmd = ws.recv ()
                        let servPath = cmd.Split(' ').[1]

                        match getServ servPath with
                        | Some (NeedAuth, handler) -> whenNeedAuthDo ws handler
                        | Some (Everyone, handler) -> whenEveryoneDo ws handler
                        | None -> log "serv rejected" //拒绝服务
                    with
                    | :? SocketException -> log "connection lost"
                    | _ -> log "auth failed"
            |> Task.RunIgnore

        self.buildPipeline.mappend (Pipe(func = func))
