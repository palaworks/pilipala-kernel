[<AutoOpen>]
module pilipala.util.hash.bcrypt

open System
open BCrypt.Net

type String with

    /// 字符串的bcrypt签名
    /// 使用随机salt
    member self.bcrypt = BCrypt.HashPassword(self)
