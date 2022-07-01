namespace pilipala.builder

open fsharper.typ.Pipe

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f

/// 构建器
type Builder() =
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
    member self.build() = self.buildPipeline.fill ()
