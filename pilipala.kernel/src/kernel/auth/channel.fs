namespace pilipala.kernel.auth

open System
open System.Net.Sockets
open System.Security.Cryptography
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open pilipala.util.crypto
open pilipala.util.socket.tcp
open pilipala.util.uuid
open pilipala.kernel.auth.token

/// 安全信道
type SecureChannel internal (s: Socket, sessionKey: string) =

    /// 向信道发送消息
    member this.sendText message =
        message
        |> aes.encrypt sessionKey PaddingMode.Zeros
        |> s.sendText

    /// 从信道接收消息
    /// 此方法阻塞当前线程
    member this.recvText() =
        s.recvText ()
        |> aes.decrypt sessionKey PaddingMode.Zeros
