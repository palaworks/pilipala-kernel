module pilipala.util.json

open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters


type Object with
    /// 序列化到json
    member self.json =
        (self, IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-dd HH:mm:ss"))
        |> JsonConvert.SerializeObject
