[<AutoOpen>]
module pilipala.util.encoding.base64

open System

// base64字符串 <-> 字节数组
let inline base64ToBytes (s: string) = Convert.FromBase64String s
let inline bytesToBase64 bytes = Convert.ToBase64String bytes

// base64字符串 <-> utf8字符串
let base64ToUtf8 (s: string) = s |> base64ToBytes |> bytesToUtf8

type String with
    member self.base64 =
        self |> utf8ToBytes |> Convert.ToBase64String
