namespace pilipala.pipeline

open fsharper.typ.Pipe.Pipable

type IRenderPipeLine<'t> =
    /// 在管道入口插入管道
    abstract Before : Pipe<'t> -> IRenderPipeLine<'t>
    /// 在管道出口插入管道
    abstract Then : Pipe<'t> -> IRenderPipeLine<'t>
