namespace pilipala.util.crypto

module rsa =

    open System
    open pilipala.kernel

    /// 加密明文
    let encrypt (pubKey: string) (plainText: string) = plainText |> cslib.RSA(pubKey).Encode
    /// 解密密文
    let decrypt (priKey: string) (cipherText: string) =
        cipherText |> cslib.RSA(priKey).DecodeOrNull

    /// 生成RSA密钥对
    let genRsaKeyPair (keySize: uint16) =
        let rsa = cslib.RSA(keySize |> int)

        {| priKey = rsa.ToPEM().ToPEM_PKCS1()
           pubKey = rsa.ToPEM().ToPEM_PKCS8(true) |}
