[<AutoOpen>]
module pilipala.builder.usePluginSet
 
open fsharper.typ
open fsharper.typ.Pipe.Pipable 
open pilipala

type palaBuilder with 

    /// 使用插件集
    member self.usePluginSet pluginDir =
        let func _ = plugin.invokePlugins pluginDir

        self.buildPipeline <- Pipe(func = func) |> self.buildPipeline.import
        self