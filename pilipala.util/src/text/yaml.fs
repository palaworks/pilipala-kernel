[<AutoOpen>]
module pilipala.util.text.yaml

open System
open System.IO
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

type Yaml = { yaml: string }

type Yaml with

    (*
    /// 反序列化
    member self.deserializeTo<'t>() =
        DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build()
            .Deserialize<'t>(self.yaml)
    *)

    /// 反序列化
    /// 由于YamlDotNet不能构造F#记录，yaml将转为json后由NewtonsoftJson执行反序列化，因而性能可能会受影响
    member self.deserializeTo<'t>() = self.intoJson().deserializeTo<'t> ()

    /// 将yaml字符串转换为json字符串
    member self.intoJson() : Json =
        { json =
            self.yaml
            |> DeserializerBuilder().Build().Deserialize
            |> SerializerBuilder().JsonCompatible().Build()
                .Serialize }

    /// 序列化
    static member serializeFrom(obj) =
        { yaml = SerializerBuilder().Build().Serialize obj }

type Object with
    /// 序列化到yaml
    member self.serializeToYaml() = Yaml.serializeFrom self
