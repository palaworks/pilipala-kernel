namespace pilipala.kernel

module auth =

    open System
    open System.Net.Sockets
    open fsharper.fn
    open fsharper.op
    open fsharper.ethType
    open fsharper.typeExt
    open fsharper.moreType
    open pilipala.util.crypto
    open pilipala.util.socket.tcp
    open pilipala.util.uuid
    open pilipala.kernel.token

    /// 安全信道
    type SecureChannel internal (s: Socket, sessionKey: string) =

        /// 向信道发送消息
        member this.send message =
            message |> aes.encrypt sessionKey |> s.send

        /// 从信道接收消息
        /// 此方法阻塞当前线程
        member this.recv() = s.recv |> aes.decrypt sessionKey


    /// 在指定端口启动认证服务
    /// 认证通过后，会以 SecureChannel 为参数执行闭包 f
    let serveOn port f =
        let cli (msg: string) =
            Console.WriteLine $"pilipala auth service : {msg}"

        Async.Start
        <| async {
            cli "service online"

            listen port
            <| fun s ->
                Console.WriteLine()
                cli "new client connected"

                try
                    match s.recv with
                    | "hello" -> //客户端问候
                        s.send "hi" //服务端问候
                        cli "start authing"

                        let pubKey = s.recv //接收客户公钥
                        cli "pubKey received"

                        let sessionKey = gen N //生成会话密钥
                        //将会话密钥使用客户公钥加密后送回
                        sessionKey |> rsa.encrypt pubKey |> s.send
                        cli "sessionKey sent"

                        //接收密文解密到凭据
                        let token = aes.decrypt sessionKey <| s.recv
                        cli "token received"

                        //凭据校验
                        match check token with
                        | Ok true ->
                            s.send "pass" //受信通告
                            cli "client certified"

                            SecureChannel(s, sessionKey) |> f

                        | _ -> cli "token invalid" //凭据无效
                    | _ -> cli "client rejected" //非login请求
                with
                | :? SocketException -> cli "connection lost"
                | _ -> cli "auth failed"
            |> ignore
           }
