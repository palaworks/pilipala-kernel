[<AutoOpen>]
module pilipala.builder

open fsharper.types
open fsharper.types.Pipe.Pipable
open pilipala

/// 构建器
type palaBuilder() =
    let mutable buildPipeline = Pipe<unit>()

    (*
    /// 使用全局缓存
    member self.useGlobalCache() = ()
    /// 使用页缓存
    member self.usePageCache() = ()
    /// 使用内存表
    member self.useMemoryTable() = ()
    /// 使用调试信息
    member self.useDebugMessage() = ()
    /// 使用日志记录
    member self.useLog logPath = ()
    *)

    /// 使用配置文件
    member self.useConfig configFilePath =
        let func () =
            config.configFilePath <- Some <| configFilePath

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self
    ///使用插件
    member self.usePlugins pluginDir =
        let func () = plugin.invokePlugins pluginDir
        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self

    /// 使用认证系统
    member self.useAuth listenPort = ()

    /// 构建
    member self.build = buildPipeline.build().invoke
