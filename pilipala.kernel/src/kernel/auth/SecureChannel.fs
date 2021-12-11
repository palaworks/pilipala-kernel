namespace pilipala.kernel.auth

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
open pilipala.kernel.auth.token

/// 安全信道
type SecureChannel internal (s: Socket, sessionKey: string) =

    /// 向信道发送消息
    member this.send message =
        message |> aes.encrypt sessionKey |> s.send

    /// 从信道接收消息
    /// 此方法阻塞当前线程
    member this.recv() = s.recv |> aes.decrypt sessionKey
