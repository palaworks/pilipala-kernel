namespace pilipala.util.encoding

open System
open System.Text
open Microsoft.IdentityModel.Tokens

[<AutoOpen>]
module byte =
    /// utf8字符串 <-> 字节数组
    let inline bytesToUtf8 bytes =
        Encoding.UTF8.GetString(bytes, 0, bytes.Length)

    let inline utf8ToBytes (s: string) = Encoding.UTF8.GetBytes s

    /// base64Url字符串 <-> utf8字符串
    let base64UrlToUtf8 (s: string) = Base64UrlEncoder.Decode s

    type String with
        member self.base64Url = self |> Base64UrlEncoder.Encode

[<AutoOpen>]
module base64 =
    // base64字符串 <-> 字节数组
    let inline base64ToBytes (s: string) = Convert.FromBase64String s
    let inline bytesToBase64 bytes = Convert.ToBase64String bytes

    // base64字符串 <-> utf8字符串
    let base64ToUtf8 (s: string) = s |> base64ToBytes |> bytesToUtf8

    type String with
        member self.base64 =
            self |> utf8ToBytes |> Convert.ToBase64String

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
