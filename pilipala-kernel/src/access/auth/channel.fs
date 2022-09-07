namespace pilipala.access.auth.channel

open System.Security.Cryptography
open System.Threading.Channels
open WebSocketSharp
open fsharper.op.Async
open pilipala.util.crypto
open pilipala.util.encoding

//TODO 应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

type NetChannel
    private
    (
        ws: WebSocket,
        encryptor: string -> string,
        decryptor: string -> string,
        recvBuffer: Channel<string>
    ) =

    /// 启用AES256加密的信道
    new(ws: WebSocket, sessionKey: string) =

        let buf = Channel.CreateUnbounded<string>()

        ws.OnMessage.Add
        <| fun e -> buf.Writer.WriteAsync e.Data |> ignore

        let sessionKeyBytes =
            sessionKey |> hexToBytes

        let en =
            aes.encrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros

        let de =
            aes.decrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros

        NetChannel(ws, en, de, buf)

    /// 没有任何安全措施的信道
    new(ws: WebSocket) =
        let buf = Channel.CreateUnbounded<string>()

        ws.OnMessage.Add
        <| fun e -> buf.Writer.WriteAsync e.Data |> ignore

        NetChannel(ws, id, id, buf)

    member self.Write message = message |> encryptor |> ws.Send

    member self.Read() =
        recvBuffer.Reader.ReadAsync().AsTask()
        |> result
        |> decryptor

(*
type UnsafeNetChannel(ws: WebSocket) =

    let nc = NetChannel(ws)

    member self.Write = nc.Write
    member self.Read = nc.Read

type SafeNetChannel(ws: WebSocket, sessionKey: string) =

    let nc = NetChannel(ws, sessionKey)

    member self.Write = nc.Write
    member self.Read = nc.Read
*)
