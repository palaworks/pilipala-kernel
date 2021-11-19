namespace pilipala.util

module yaml =

    open System
    open System.IO
    open YamlDotNet.Serialization

    type String with
        /// 将yaml字符串转换为json字符串
        member self.yamlInJson =
            new StringReader(self)
            |> DeserializerBuilder().Build().Deserialize
            |> SerializerBuilder().JsonCompatible().Build()
                .Serialize
