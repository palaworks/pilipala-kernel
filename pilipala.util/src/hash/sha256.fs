[<AutoOpen>]
module pilipala.util.hash.sha256

open System
open System.Security.Cryptography
open pilipala.util.encoding

type String with

    /// 字符串的sha256签名
    member self.sha256 =
        self
        |> utf8ToBytes
        |> SHA256.Create().ComputeHash
        |> bytesToHex
