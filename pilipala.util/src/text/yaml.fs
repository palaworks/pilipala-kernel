[<AutoOpen>]
module pilipala.util.text.yaml

open System
open System.IO
open YamlDotNet.Serialization

type Yaml = { yaml: string }

type Yaml with
    /// 将yaml字符串转换为json字符串
    member self.intoJson: Json =
        { json =
            new StringReader(self.yaml)
            |> DeserializerBuilder().Build().Deserialize
            |> SerializerBuilder().JsonCompatible().Build()
                .Serialize }
