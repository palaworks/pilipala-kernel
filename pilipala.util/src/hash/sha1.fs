[<AutoOpen>]
module pilipala.util.hash.sha1

open System
open System.Security.Cryptography
open pilipala.util.encoding

type String with

    /// 字符串的sha1签名
    member self.sha1 =
        self
        |> utf8ToBytes
        |> SHA1.Create().ComputeHash
        |> bytesToHex
