module pilipala.util.encoding

open System
open System.Text
open Microsoft.IdentityModel.Tokens


/// 字节数组转utf8字符串
let bytesToUtf8 bytes =
    Encoding.UTF8.GetString(bytes, 0, bytes.Length)
/// utf8字符串转字节数组
let utf8ToBytes (utf8: string) = Encoding.UTF8.GetBytes utf8

/// 解码16进制字符串
let decodeHex (hex: string) =
    hex |> Convert.FromHexString |> bytesToUtf8

/// 解码base64
let decodeBase64 (base64: string) =
    base64 |> Convert.FromBase64String |> bytesToUtf8

/// 解码base64url
let decodeBase64url (base64url: string) = base64url |> Base64UrlEncoder.Decode

type String with

    /// 转换到16进制字符串
    member self.hex = self |> utf8ToBytes |> Convert.ToHexString

    /// 转换到base64字符串
    member self.base64 =
        self |> utf8ToBytes |> Convert.ToBase64String

    /// 转换到适用于url的base64字符串
    member self.base64url = self |> Base64UrlEncoder.Encode
