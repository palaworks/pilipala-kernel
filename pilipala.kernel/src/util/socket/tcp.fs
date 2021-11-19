namespace pilipala.util.socket

open K4os.Compression.LZ4.Internal

module tcp =

    open System
    open System.Text
    open System.Net
    open System.Net.Sockets
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

        /// 发送文本消息
        member self.send(msg: string) = msg |> getBytes |> self.Send |> ignore

        /// 接收文本消息
        member self.recv =
            let buf = Array.zeroCreate<byte> 4096

            let rec f (sb: StringBuilder) =
                match self.Receive buf with
                | n when n = buf.Length ->
                    Encoding.UTF8.GetString(buf, 0, n)
                    |> sb.Append
                    |> f
                | n -> //缓冲区未满，说明全部接收完毕
                    Encoding.UTF8.GetString(buf, 0, n) |> sb.Append

            (StringBuilder() |> f).ToString()
