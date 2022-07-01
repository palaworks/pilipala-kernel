namespace pilipala.pipeline

open fsharper.typ.Pipe

/// 渲染管道
type IRenderPipeLine<'I, 'O> =
    /// 在管道入口插入管道（数据库获取之前）
    abstract Before : IPipe<'I> -> IRenderPipeLine<'I, 'O>
    /// 在管道出口插入管道（数据库获取之后）
    abstract After : IPipe<'O> -> IRenderPipeLine<'I, 'O>

    /// 替换管道
    abstract Replace : (IGenericPipe<'I, 'O> -> IGenericPipe<'I, 'O>) -> IRenderPipeLine<'I, 'O>

/// 存储管道
type IStoragePipeLine<'T> =
    /// 在管道入口插入管道（数据库写入之前）
    abstract Before : IPipe<'T> -> IStoragePipeLine<'T>
    /// 在管道出口插入管道（数据库写入之后）
    abstract After : IPipe<'T> -> IStoragePipeLine<'T>

    /// 替换管道
    abstract Replace : (IPipe<'T> -> IPipe<'T>) -> IStoragePipeLine<'T>
