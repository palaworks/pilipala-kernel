namespace pilipala.auth.channel

open System.Security.Cryptography
open WebSocketer.typ
open pilipala.util.crypto
open pilipala.util.encoding

//TODO 应使用随机化IV+CBC以代替ECB模式以获得最佳安全性

type NetChannel private (ws: WebSocket, encryptor: string -> string, decryptor: string -> string) =

    /// 启用AES256加密的信道
    new(ws: WebSocket, sessionKey: string) =
        let sessionKeyBytes = sessionKey |> hexToBytes

        let en =
            aes.encrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros

        let de =
            aes.decrypt sessionKeyBytes [||] CipherMode.ECB PaddingMode.Zeros

        NetChannel(ws, en, de)

    /// 没有任何安全措施的信道
    new(ws: WebSocket) = NetChannel(ws, id, id)

    member self.Write message = message |> encryptor |> ws.send

    member self.Read() = ws.recv () |> decryptor

type UnsafeNetChannel(ws: WebSocket) =

    let nc = NetChannel(ws)

    member self.Write = nc.Write
    member self.Read = nc.Read

type SafeNetChannel(ws: WebSocket, sessionKey: string) =

    let nc = NetChannel(ws, sessionKey)

    member self.Write = nc.Write
    member self.Read = nc.Read
