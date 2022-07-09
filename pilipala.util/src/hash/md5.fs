namespace pilipala.util.hash

open System
open System.Security.Cryptography
open pilipala.util.encoding

[<AutoOpen>]
module md5 =

    type String with

        /// 字符串的md5签名
        member self.md5 =
            self
            |> utf8ToBytes
            |> MD5.Create().ComputeHash
            |> bytesToHex
