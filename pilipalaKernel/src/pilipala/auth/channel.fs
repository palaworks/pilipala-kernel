module pilipala.auth.channel

open System
open System.Security.Cryptography
open pilipala.util.crypto
open WebSocketer.Type


/// 安全信道
type SecureChannel internal (s: WebSocket, sessionKey: string) =
    let sessionKeyBytes = sessionKey |> Convert.FromHexString

    //TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

    /// 向信道发送消息
    member this.sendMsg message =
        message
        |> aes.encrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros
        |> s.send

    /// 从信道接收消息
    /// 此方法阻塞当前线程
    member this.recvMsg() =
        s.recv ()
        |> aes.decrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros
