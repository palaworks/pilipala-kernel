[<AutoOpen>]
module pilipala.builder.usePluginSet
 
open fsharper.typ
open fsharper.typ.Pipe.Pipable 
open pilipala

type palaBuilder with 

    /// 使用插件
    member self.usePlugin pluginDir =
        let func _ = plugin.invokePlugin pluginDir

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self