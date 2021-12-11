module pilipala.util.crypto.aes

open System
open System.Text
open System.Security.Cryptography
open pilipala.util.encoding

/// key长度必须为32

/// 加密明文
let encrypt (key: string) (plainText: string) =
    let keyBytes = getBytes key
    let plainBytes = getBytes plainText

    //采用ECB+零填充
    let encryptor = //加密器
        (new RijndaelManaged(BlockSize = 128, Key = keyBytes, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros))
            .CreateEncryptor()

    let cipherBytes = //密文字节组
        encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length)

    Convert.ToHexString(cipherBytes, 0, cipherBytes.Length)

/// 解密密文
let decrypt (key: string) (cipherText: string) =
    let keyBytes = getBytes key
    let cipherBytes = cipherText |> Convert.FromHexString //密文是16进制文本

    //采用ECB+零填充
    let decryptor = //解密器
        (new RijndaelManaged(BlockSize = 128, Key = keyBytes, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros))
            .CreateDecryptor()

    let plainBytes = //明文字节组
        decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length)

    plainBytes |> Encoding.UTF8.GetString
