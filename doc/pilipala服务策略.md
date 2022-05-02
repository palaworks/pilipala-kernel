# pilipala服务策略

## 创建凭据

* 服务器生成一个UUID作为凭据：  
`e23482239fdf45d1b01b3b4fa3721c37`
* 服务器将计算到该UUID的哈希（sha1）：  
`6D8AE03878DB3361FB70462C6AC36E73C6247537`
* 服务器将该哈希存入凭据数据库，形成一条凭据记录。
* 服务器将该UUID展示给客户（由其他可信信道分发）。
* 客户记录下该UUID，作为凭据。

## 前提

* 客户端与服务器建立WebSocket连接。

## 服务选择

* 客户端向服务器请求服务：  
`useserv <serv_name>`
  * 若`serv_name`是公共服务，则服务器会送回服务内容，并关闭连接。
  * 若`serv_name`是私有服务，则服务器将按照下述流程继续。
  * 若`serv_name`非法，则服务器返回`bad serv`，并关闭连接。

## 认证

* 服务器收到请求，向客户端回应需要认证：  
`need auth`
* 客户端生成一对RSA密钥对（2048位，PEM格式，公钥采用PKCS1，私钥采用PKCS8），将公钥发送给服务器：

```text
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAn9bHPcm24u2cm1MF1G4A
/crJmjk1aDWa6LfyNHSHzFTQhP/cF6XiVu+zXYij9hwp8I10045ywgfwcNLR2+te
QqjbR7K/nYGB37Bh0B3u0afi30aXEqltA+HQGBm7sxEYfmzvFLVgs6jQlYtM9XNU
H0FhTIXxBppfh0t8Nn3NKWJVcgomsfrQTsvVqF16TDOudeEYT7UmjyKT7rD1iu3p
YJ5ELqJ8TWnt1sHOpwcC4RW1lA/q6svVxfYaRr+6BveUZ9y/bKZCTj8Tb3MOc2GI
lQg1oygMVTQJncRmkqdX0qqhtd8B5RhIYTr4BiDiBcnYHZFz79it9Bzmu1oif6F1
jQIDAQAB
-----END PUBLIC KEY-----
```

* 服务器收到该公钥。
* 服务器生成一个UUID作为会话密钥：  
`6c953ee774a849e9b60c3d8a12473a6b`
* 服务器使用客户端公钥将会话密钥加密（PKCS1 OAEP填充）后送回客户端：

```text
qV6IYIOIhOBaHEpIyOX7zPVjLHax2AC+xsyzJuvLEhoJqOvvj0n4jXQC1J2Dbx9yP/y3UybccSbXj7ut+EW0UORC6rA3sMA/R+p2F5hlgJhPdzDLKoOvpWxtROq23OX6i4yoqZ3MoituqGICBxoVopz30TVc3Y+ayHPYYrkNvYc5PBZu2BGgPmM/CLiuc1dQ3o6fJ0LkamXkiBqTu4e9lwEOLJaE2ht+VGW0zue6ecIlUEjHrcU37m9kLlFsEeAweDtg3ppoKtnEVziy46m6ygnCbbxUbamxGdq+N6AFuCWl/C5VY7B05YOMCV9KNZgv6SE2LcUaHhSbrZTc/OFSTg==
```

* 客户端收到该密文，使用私钥解密得到会话密钥：
`6c953ee774a849e9b60c3d8a12473a6b`
* 客户使用会话密钥，将凭据使用AES256加密后发往服务器：
`517acf1cf35ff1ba6dfccfdf317d9a8c89ee7ddcfab6845ed365b7b81d596d3b1b590679a45560be9c94d40c607b8613`
* 服务器收到密文，使用会话密钥解密得到客户凭据：  
`e23482239fdf45d1b01b3b4fa3721c37`
* 服务器计算该凭据的哈希（sha1）：  
`6D8AE03878DB3361FB70462C6AC36E73C6247537`
* 服务器检索凭据数据库，发现匹配的凭据记录。
* 服务器判定客户受信，返回受信通告：  
`auth pass`

## 使用服务

例如，客户需将下述数据发往服务器：

```text
create comment
```

* 客户使用会话密钥将数据加密后发往服务器：
`bebf6af8415db6af6e7d169b64bcdb68f778cd7789ce1c95f2ab5ae92934c906ac233a2fd26da2085aa007f6ff40bb48f78f9138ecdbfd57085ce31da3735492`
* 服务器收到数据后，使用会话密钥解密得到数据：

```text
create comment
```

* 服务器将数据交由请求服务处理，得到处理结果：

```text
new comment was created with id 1450455144255852544
```

* 服务器使用会话密钥加密处理结果，送回客户。

## 会话结束判定

* 服务器将维持一个心跳以判定连接的可用性，当连接不可用时服务器会关闭该连接，相关服务也会终止。
