namespace pilipala.util

open System
open System.Text

module encoding =

    open System
    open System.Text
    open System.IO
    open YamlDotNet.Serialization
    open Microsoft.IdentityModel.Tokens

    /// 统一使用utf8编码

    let getBytes (str: string) = Encoding.UTF8.GetBytes str

    /// 解码16进制字符串
    let decodeHex (hex: string) =
        hex
        |> Convert.FromHexString
        |> Encoding.UTF8.GetString

    /// 解码base64
    let decodeBase64 (base64: string) =
        base64
        |> Convert.FromBase64String
        |> Encoding.UTF8.GetString

    /// 解码base64url
    let decodeBase64url (base64url: string) = base64url |> Base64UrlEncoder.Decode

    type String with

        /// 转换到16进制字符串
        member self.hex = self |> getBytes |> Convert.ToHexString

        /// 转换到base64字符串
        member self.base64 =
            self |> getBytes |> Convert.ToBase64String

        /// 转换到适用于url的base64字符串
        member self.base64url = self |> Base64UrlEncoder.Encode
