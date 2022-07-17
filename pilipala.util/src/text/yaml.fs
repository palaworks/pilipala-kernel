module pilipala.util.yaml

open System
open System.IO
open YamlDotNet.Serialization
open pilipala.util.json

type Yaml = { value: string }

type Yaml with
    /// 将yaml字符串转换为json字符串
    member self.intoJson: Json =
        { value =
            new StringReader(self.value)
            |> DeserializerBuilder().Build().Deserialize
            |> SerializerBuilder().JsonCompatible().Build()
                .Serialize }
