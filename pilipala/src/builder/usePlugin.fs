[<AutoOpen>]
module pilipala.builder.usePluginSet

open fsharper.typ
open fsharper.typ.Pipe.Pipable
open pilipala

type Builder with

    /// 使用插件
    member self.usePlugin path =
        let func _ = plugin.invokePlugin path

        self.buildPipeline.mappend (Pipe(func = func))
