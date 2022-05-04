namespace pilipala.builder

open System
open System.IO
open System.Reflection
open System.Threading.Tasks
open Google.Protobuf.WellKnownTypes
open fsharper.types
open fsharper.types.Pipe.Pipable
open pilipala.service
open pilipala

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f


/// 构建器
type palaBuilder() =
    let mutable buildPipeline = Pipe<unit>()
    let mutable logStream = new MemoryStream()

    let mutable servLogStream = new MemoryStream()

    (*
    /// 使用全局缓存
    member self.useGlobalCache() = ()
    /// 使用页缓存
    member self.usePageCache() = ()
    /// 使用内存表
    member self.useMemoryTable() = ()
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

    /// 使用日志
    member self.useLog logStream = ()
    /// 使用服务日志
    member self.useServiceLog logStream = ()

    /// 使用公共服务
    member self.useService<'s>() =
        let func _ = regService<'s> servLogStream

        buildPipeline <- Pipe(func = func) |> buildPipeline.import
        self


    /// 构建
    member self.build = buildPipeline.build().invoke
