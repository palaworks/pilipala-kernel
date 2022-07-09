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
