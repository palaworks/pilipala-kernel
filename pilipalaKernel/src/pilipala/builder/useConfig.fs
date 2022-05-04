[<AutoOpen>]
module pilipala.builder.useConfig
 
open fsharper.typ
open fsharper.typ.Pipe.Pipable 
open pilipala

type palaBuilder with

    /// 使用配置文件
    member self.useConfig configFilePath =
        let func _ =
            config.configFilePath <- Some <| configFilePath

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self
