module pilipala.util.socket.tcp

open System
open System.Net
open System.Net.Sockets
open fsharper.typ.Array
open pilipala.util.encoding

/// 与指定ip端口建立tcp连接
let connect (ip: string) (port: uint16) =
    let socket =
        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

    let endPoint = IPEndPoint(IPAddress.Parse ip, int port)

    try
        socket.Connect endPoint
        Ok socket
    with
    | e -> Error e

/// 持续监听本机指定端口的tcp连接
/// 闭包 f 生命期结束后其连接会被自动销毁
/// 此函数会永久性阻塞当前线程
let listen (port: uint16) f =
    let listenSocket =
        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

    let endPoint = IPEndPoint(IPAddress.Any, int port)

    listenSocket.Bind endPoint
    listenSocket.Listen 0 //TODO

    try
        while true do
            let s = listenSocket.Accept()
            f s
            s.Dispose()

        Ok()
    with
    | e ->
        listenSocket.Dispose()
        Error e

type Socket with

    /// 发送字节数据
    member self.sendBytes(bytes: byte array) =
        match self.Send bytes with
        | sentLen when sentLen = bytes.Length -> () //全部发送完成
        | sentLen -> self.sendBytes bytes.[sentLen..^0] //继续发送剩余部分

    /// 接收指定长度字节数据
    member self.recvBytes(n: uint32) =

        let rec fetch buf start remain =
            match self.Receive(buf, start, remain, SocketFlags.None) with
            | readLen when readLen = remain -> //读完
                buf
            | readLen -> fetch buf readLen (remain - readLen)

        let n' = min (uint32 Int32.MaxValue) n |> int //防止溢出

        fetch (Array.zeroCreate<byte> n') 0 n'

    /// 接收所有字节数据
    member self.recvAllBytes() =
        let buf = Array.zeroCreate<byte> 4096

        let rec fetch acc =
            match self.Receive buf with
            | readLen when readLen = buf.Length -> //尚未读完
                acc @ buf.toList () |> fetch
            | readLen -> //缓冲区未满，说明全部接收完毕
                acc @ buf.[0..readLen - 1].toList ()

        [] |> fetch |> List.toArray

type Socket with

    /// 发送UTF8数据
    member self.sendUtf8(s: string) = s |> utf8ToBytes |> self.sendBytes

    /// 以UTF8编码接收所有数据
    member self.recvAllUtf8() = self.recvAllBytes () |> bytesToUtf8
