module pilipala.util.json

open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Linq

type Json = { value: string }

type Json with
    /// 反序列化
    member self.deserializeTo<'t>() =
        JsonConvert.DeserializeObject<'t> self.value

type Object with
    /// 序列化到json
    member self.serializeToJson =
        { value =
            (self, IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-dd HH:mm:ss"))
            |> JsonConvert.SerializeObject }

type String with

    member self.parseToJObject = JObject.Parse self
