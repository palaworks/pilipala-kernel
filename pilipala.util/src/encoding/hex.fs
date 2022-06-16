namespace pilipala.util.encoding

open System

[<AutoOpen>]
module hex =
    // hex字符串 <-> 字节数组
    let inline hexToBytes (s: string) = Convert.FromHexString s
    let inline bytesToHex (bytes: byte []) = bytes |> Convert.ToHexString

    // hex字符串 <-> utf8字符串
    let hexToUtf8 (s: string) = s |> hexToBytes |> bytesToUtf8

    type String with
        member self.hex =
            self |> utf8ToBytes |> Convert.ToHexString
