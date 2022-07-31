namespace pilipala.pipeline

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Foldable

type PipelineCombineMode<'I, 'O> =
    /// 在管道入口插入管道（数据库获取之前）
    | Before of ('I -> 'I)
    /// 替换管道
    | Replace of (('I -> 'O) -> 'I -> 'O)
    /// 在管道出口插入管道（数据库获取之后）
    | After of ('O -> 'O)

type BuilderItem<'I, 'O> =
    { collection: PipelineCombineMode<'I, 'O> List
      beforeFail: ('I -> 'I) List }

type BuilderItem<'T> = BuilderItem<'T, 'T>


[<AutoOpen>]
module ext_BuilderItem =

    type BuilderItem<'I, 'O> with
        member inline self.fullyBuild basePipe =
            let fail =
                (self.beforeFail.foldr (<.) id) //before fail
                .> panicwith

            self.collection.foldl
            <| fun acc x ->
                match x with
                | Before before -> before .> acc
                | Replace f -> f acc
                | After after -> acc .> after
            <| basePipe fail

        member inline self.noneBeforeBuild basePipe =
            let fail =
                (self.beforeFail.foldr (<.) id) //before fail
                .> panicwith

            self.collection.foldl
            <| fun acc x ->
                match x with
                | Before _ -> acc
                | Replace f -> f acc
                | After after -> acc .> after
            <| basePipe fail

        member inline self.noneAfterBuild basePipe =
            let fail =
                (self.beforeFail.foldr (<.) id) //before fail
                .> panicwith

            self.collection.foldl
            <| fun acc x ->
                match x with
                | Before before -> before .> acc
                | Replace f -> f acc
                | After _ -> acc
            <| basePipe fail
