module internal pilipala.config

open System.IO
open fsharper.types
open fsharper.op.Boxing
open fsharper.types.Pipe.GenericPipable
open pilipala.util.yaml
open pilipala.util.json
open pilipala.util.io
open Newtonsoft.Json.Linq


//配置文件路径
let mutable configFilePath: Option'<string> = None

let mutable private rootNode: Option'<JObject> = None

let private configPipeline =

    let fetch () =
        let config = configFilePath |> unwarp |> readFile

        let _rootNode = config.jsonParsed
        rootNode <- Some <| _rootNode

        _rootNode

    let provide () = rootNode |> unwarp

    GenericStatePipe(activate = fetch, activated = provide)
        .build ()

let JsonConfig = configPipeline.invoke
