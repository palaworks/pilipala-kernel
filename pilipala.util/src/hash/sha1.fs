namespace pilipala.util.hash

open System
open System.Security.Cryptography
open pilipala.util.encoding

[<AutoOpen>]
module sha1 =

    type String with

        /// 字符串的sha1签名
        member self.sha1 =
            self
            |> utf8ToBytes
            |> SHA1.Create().ComputeHash
            |> bytesToHex
