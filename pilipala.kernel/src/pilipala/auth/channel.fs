module pilipala.auth.channel

open System
open System.Net.Sockets
open System.Security.Cryptography
open pilipala.util.crypto
open pilipala.util.socket.tcp


/// 安全信道
type SecureChannel internal (s: Socket, sessionKey: string) =
    let sessionKeyBytes = sessionKey |> Convert.FromHexString

    //TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

    /// 向信道发送消息
    member this.sendText message =
        message
        |> aes.encrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros
        |> s.sendText

    /// 从信道接收消息
    /// 此方法阻塞当前线程
    member this.recvText() =
        s.recvText ()
        |> aes.decrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros
