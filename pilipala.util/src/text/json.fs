[<AutoOpen>]
module pilipala.util.text.json

open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Linq

type Json = { json: string }

type Json with
    /// 反序列化
    member self.deserializeTo<'t>() =
        JsonConvert.DeserializeObject<'t> self.json

type Object with
    /// 序列化到json
    member self.serializeToJson =
        { json =
            (self, IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-dd HH:mm:ss"))
            |> JsonConvert.SerializeObject }

(*
type String with

    member self.parseToJObject = JObject.Parse self
*)