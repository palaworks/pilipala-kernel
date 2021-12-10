namespace pilipala.util

module hash =

    open System
    open System.Text
    open System.Security.Cryptography

    type Object with
        /// 转换到指定哈希算法的字符串
        member self.hash(hasher: HashAlgorithm) =
            let bytes =
                self
                |> Convert.ToString
                |> Encoding.Default.GetBytes //转换成字节数组
                |> hasher.ComputeHash

            let builder = StringBuilder() //用于收集字节

            for byte in bytes do
                byte.ToString "X2" //格式每一个十六进制字符串
                |> builder.Append
                |> ignore

            builder.ToString().ToLower()

        /// 转换到 md5 字符串
        member self.md5 = MD5.Create() |> self.hash
        /// 转换到 sha1 字符串
        member self.sha1 = SHA1.Create() |> self.hash
        /// 转换到 sha256 字符串
        member self.sha256 = SHA256.Create() |> self.hash
