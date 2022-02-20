module pilipala.util.crypto.rsa

open System
open System.Text
open System.Security.Cryptography

/// 加密明文
let encrypt (pubKey: string) (paddingMode: RSAEncryptionPadding) (plainText: string) =
    let csp = new RSACryptoServiceProvider()
    csp.ImportFromPem pubKey

    let plainBytes = plainText |> Encoding.UTF8.GetBytes
    let cipherBytes = csp.Encrypt(plainBytes, paddingMode)

    cipherBytes |> Convert.ToBase64String

/// 解密密文
let decrypt (priKey: string) (paddingMode: RSAEncryptionPadding) (cipherText: string) =
    let csp = new RSACryptoServiceProvider()
    csp.ImportFromPem priKey

    let cipherBytes = cipherText |> Convert.FromBase64String
    let plainBytes = csp.Decrypt(cipherBytes, paddingMode)

    plainBytes |> Encoding.UTF8.GetString

/// 生成RSA密钥对
let genRsaKeyPair (keySize: uint16) =
    let csp =
        new RSACryptoServiceProvider(int keySize)

    let priKey =
        csp.ExportPkcs8PrivateKey()
        |> Convert.ToBase64String

    let pubKey =
        csp.ExportSubjectPublicKeyInfo()
        |> Convert.ToBase64String

    let rec breakToLines (key: string) start len =
        if start + len < key.Length then
            $"{key.Substring(start, len)}\n{breakToLines key (start + len) len}"
        elif start + len > key.Length then
            breakToLines key start (key.Length - start)
        else //start + len = key.Length
            key.Substring(start, len)

    {| priKey = $"-----BEGIN PRIVATE KEY-----\n{breakToLines priKey 0 64}\n-----END PRIVATE KEY-----"
       pubKey = $"-----BEGIN PUBLIC KEY-----\n{breakToLines pubKey 0 64}\n-----END PUBLIC KEY-----" |}
