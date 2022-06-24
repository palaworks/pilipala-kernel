namespace pilipala.util.hash

open System
open BCrypt.Net

[<AutoOpen>]
module bcrypt =

    type String with

        /// 字符串的bcrypt签名
        /// 使用随机salt
        member self.bcrypt = BCrypt.HashPassword(self)
