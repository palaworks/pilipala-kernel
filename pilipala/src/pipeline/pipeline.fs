namespace pilipala.pipeline

open System.Collections.Generic
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Runtime
open fsharper.op.Foldable
open pilipala.pipeline

[<AutoOpen>]
module internal helper =

    let fullyBuild data (initBuilderItem: BuilderItem<_, _>) =
        let fail =
            (initBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>()))
                .fill //before fail
            .> panicwith

        initBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    let noBeforeBuild data (initBuilderItem: BuilderItem<_, _>) =
        let fail =
            (initBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>()))
                .fill //before fail
            .> panicwith

        initBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before _ -> acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    let noAfterBuild data (initBuilderItem: BuilderItem<_, _>) =
        let fail =
            (initBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>()))
                .fill //before fail
            .> panicwith

        initBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After _ -> acc
        <| GenericCachePipe<_, _>(data, fail)
