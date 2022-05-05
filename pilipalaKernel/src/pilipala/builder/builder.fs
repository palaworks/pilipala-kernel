﻿namespace pilipala.builder

open System
open System.Collections.Generic
open System.IO
open fsharper.typ.Pipe.Pipable
open pilipala.util.stream

/// 在指定端口启动认证服务
/// 认证通过后，会以 SecureChannel 为参数执行闭包 f

/// 构建器
type palaBuilder() =
    /// 构建函数管道
    member val internal buildPipeline = Pipe<unit>() with get, set
    /// 日志流生成器集合
    member val internal logStreamGetterList: (unit -> Stream) list = [] with get, set
    (*
    /// 使用页缓存
    member self.usePageCache() = ()
    /// 使用内存表
    member self.useMemoryTable() = ()
    *)

    (*
    内核构造顺序：
    useConfig
    usePluginSet
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
    member self.build = self.buildPipeline.build().invoke
