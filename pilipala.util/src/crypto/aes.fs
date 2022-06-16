module pilipala.util.crypto.aes

open System
open System.IO
open System.Security.Cryptography
open pilipala.util.encoding


/// 加密明文
/// key and iv is 128bit
/// output is UTF8 encoded hex string
let encrypt (key: byte []) (iv: byte []) mode paddingMode (plainText: string) =

    use encryptor =
        Aes
            .Create(Key = key, IV = iv, Padding = paddingMode, Mode = mode)
            .CreateEncryptor()

    use ms = new MemoryStream()

    use cs =
        new CryptoStream(ms, encryptor, CryptoStreamMode.Write)

    use sw = new StreamWriter(cs)

    plainText |> sw.Write

    sw.Flush() //应用缓冲
    cs.Close() //关闭是必须的，这样可以刷新流，并保证所有剩余块被处理

    let cipherBytes = ms.ToArray()
    cipherBytes |> Convert.ToHexString

/// 解密密文
/// key and iv is 128bit
/// output is UTF8 encoded string
let decrypt (key: byte []) (iv: byte []) mode paddingMode (cipherText: string) =

    use decryptor =
        Aes
            .Create(Key = key, IV = iv, Padding = paddingMode, Mode = mode)
            .CreateDecryptor()

    let cipherBytes = cipherText |> hexToBytes

    use ms = new MemoryStream(cipherBytes)

    use cs =
        new CryptoStream(ms, decryptor, CryptoStreamMode.Read)

    use sr = new StreamReader(cs)

    let plainText = sr.ReadToEnd()

    plainText
