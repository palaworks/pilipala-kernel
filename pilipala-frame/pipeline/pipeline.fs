namespace pilipala.pipeline

open System
open System.Collections.Generic
open fsharper.op.Alias
open fsharper.typ.Pipe

type PipelineCombineMode<'I, 'O> =
    /// 在管道入口插入管道（数据库获取之前）
    | Before of IPipe<'I>
    /// 替换管道
    | Replace of (IGenericPipe<'I, 'O> -> IGenericPipe<'I, 'O>)
    /// 在管道出口插入管道（数据库获取之后）
    | After of IPipe<'O>

type BuilderItem<'I, 'O> =
    { collection: PipelineCombineMode<'I, 'O> List
      beforeFail: IGenericPipe<'I, 'I> List }

type BuilderItem<'T> = BuilderItem<'T, 'T>
