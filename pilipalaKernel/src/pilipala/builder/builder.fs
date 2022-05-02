namespace pilipala.builder

open System
open System.Threading.Tasks
open fsharper.types
open fsharper.types.Pipe.Pipable
open pilipala.service
open pilipala

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f


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
    *)

    /// 使用配置文件
    member self.useConfig configFilePath =
        let func _ =
            config.configFilePath <- Some <| configFilePath

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self

    /// 使用插件集
    member self.usePlugins pluginDir =
        let func _ = plugin.invokePlugins pluginDir

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self

    /// 使用认证
    member self.useAuth port =
        let func _ =
            fun _ ->
                while true do
                    useAuth port
            |> Task.Run
            |> ignore

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self

    /// 使用服务日志
    member self.useServiceLog logDir = ()

    /// 使用服务
    member self.useService serviceName serviceType serviceHandler =
        let func _ =
            regService serviceName serviceType serviceHandler

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self



    /// 使用日志
    member self.useLog logDir = ()

    /// 构建
    member self.build = buildPipeline.build().invoke
