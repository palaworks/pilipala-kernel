module pilipala.util.json

open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Linq


type Object with
    /// 序列化到json字符串
    member self.jsonSerialized =
        (self, IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-dd HH:mm:ss"))
        |> JsonConvert.SerializeObject

type String with
    /// 反序列化到JObject
    member self.jsonParsed = JObject.Parse self
