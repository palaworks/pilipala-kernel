namespace pilipala.builder

open System.IO
open fsharper.typ.Pipe.Pipable
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f

/// 构建器
type palaBuilder() =
    /// 构建函数管道
    member val internal buildPipeline = Pipe<unit>() with get, set

    (*
    /// 使用页缓存
    member self.usePageCache() = ()
    /// 使用内存表
    member self.useMemoryTable() = ()
    *)

    (*
    内核构造序：
    useDb
    usePlugin
    useAuth
    useLog
    useLog
    useLog
    useService
    useService
    useService
    usePostCache
    useCommentCache
    *)

    /// 构建
    member self.build() =
        //TODO
        let host = Host.CreateDefaultBuilder().Build()

        self
            .buildPipeline
            .mappend(Pipe<unit>(func = fun _ -> host.Run()))
            .build ()
