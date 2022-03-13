module internal pilipala.config

open System.IO
open fsharper.types
open fsharper.types.GenericPipable
open pilipala.util.yaml
open Newtonsoft.Json.Linq


//配置文件路径
let mutable configFilePath: Option'<string> = None

let mutable private rootNode: Option'<JObject> = None

let private configPipeline =

    let fetch () =
        let config =
            File.ReadAllText(configFilePath.unwarp (), System.Text.Encoding.UTF8)

        let _rootNode = config.yamlInJson |> JObject.Parse
        rootNode <- Some <| _rootNode

        _rootNode

    let provide () = rootNode.unwarp ()

    GenericStatePipe(activate = fetch, activated = provide)
        .build ()

let JsonConfig = configPipeline.invoke
