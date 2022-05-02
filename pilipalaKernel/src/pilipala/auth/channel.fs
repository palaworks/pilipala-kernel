module pilipala.auth.channel

open System
open System.Security.Cryptography
open pilipala.util.crypto
open pilipala.util.encoding
open WebSocketer.Type

/// 信道接口
type ServChannel =

    /// 向信道发送消息
    abstract member sendMsg : string -> unit
    /// 从信道接收消息
    abstract member recvMsg : unit -> string

/// 私有信道
/// 该信道用于私有服务
type PriChannel(s: WebSocket, sessionKey: string) =
    let sessionKeyBytes = sessionKey |> Convert.FromHexString

    //TODO 应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

    interface ServChannel with

        member this.sendMsg message =
            message
            |> aes.encrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros
            |> s.send

        /// 此方法阻塞当前线程
        member this.recvMsg() =
            s.recv ()
            |> aes.decrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros

/// 公共信道
/// 该信道用于公共服务
type PubChannel(s: WebSocket) =

    interface ServChannel with
        member this.sendMsg message = message |> s.send

        member this.recvMsg() = s.recv ()
