module internal pilipala.config

open System.IO
open fsharper.ethType.ethOption
open pilipala.util.yaml
open Newtonsoft.Json.Linq
open fsharper.moreType.GenericPipable

//配置文件路径
let mutable configFilePath: Option<string> = None

let mutable private rootNode: Option<JObject> = None

let private fetchConfig () =
    let config =
        File.ReadAllText(configFilePath.unwarp (), System.Text.Encoding.UTF8)

    let _rootNode = config.yamlInJson |> JObject.Parse
    rootNode <- Some <| _rootNode
    _rootNode

let private provideConfig () = rootNode.unwarp ()

let private configPipeline =
    GenericStatePipe(activate = fetchConfig, activated = provideConfig)
        .build ()

let JsonConfig = configPipeline.invoke
