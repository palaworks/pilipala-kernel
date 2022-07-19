[<AutoOpen>]
module pilipala.util.hash.sha512

open System
open System.Security.Cryptography
open pilipala.util.encoding

type String with

    /// 字符串的sha512签名
    member self.sha512 =
        self
        |> utf8ToBytes
        |> SHA512.Create().ComputeHash
        |> bytesToHex
