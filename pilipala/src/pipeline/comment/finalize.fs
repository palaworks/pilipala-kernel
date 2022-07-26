namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.comment
open pilipala.container.comment

module CommentFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentInitPipelineBuilder with
            member i.Batch = gen () }

type CommentFinalizePipeline
    internal
    (
        renderBuilder: ICommentRenderPipelineBuilder,
        modifyBuilder: ICommentModifyPipelineBuilder,
        finalizeBuilder: ICommentFinalizePipelineBuilder,
        db: IDbOperationBuilder
    ) =

    let udf_render_no_after = //去除了After部分的udf渲染管道
        let map =
            Dictionary<string, IGenericPipe<u64, u64 * obj>>()

        for kv in renderBuilder do
            let renderBuilderItem = kv.Value

            let fail =
                (renderBuilderItem.beforeFail.foldr
                    (fun p (acc: IGenericPipe<_, _>) -> acc.export p)
                    (GenericPipe<_, _>(id)))
                    .fill //before fail
                .> panicwith

            let pipe =
                renderBuilderItem.collection.foldl
                <| fun acc x ->
                    match x with
                    | Before before -> before.export acc
                    | Replace f -> f acc //视所有Replace组合为数据源
                    | _ -> acc
                <| GenericCachePipe<_, _>(always None, fail)

            map.Add(kv.Key, pipe)

        map

    let udf_modify_no_after = //去除了After部分的udf修改管道
        let map =
            Dictionary<string, IPipe<u64 * obj>>()

        for kv in modifyBuilder do
            let modifyBuilderItem = kv.Value

            let fail =
                (modifyBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>(id)))
                    .fill //before fail
                .> panicwith

            let pipe =
                modifyBuilderItem.collection.foldl
                <| fun acc x ->
                    match x with
                    | Before before -> before.export acc
                    | Replace f -> f acc
                    | _ -> acc
                <| CachePipe<_>(always None, fail)

            map.Add(kv.Key, pipe)

        map

    let data (comment_id: u64) =
        let db_data =
            db {
                inComment
                getFstRow "comment_id" comment_id
                execute
            }
            |> unwrap

        let post =
            { new IComment with
                member i.Id = comment_id

                member i.Body
                    with get () = coerce db_data.["comment_body"]
                    and set v = ()

                member i.CreateTime
                    with get () = coerce db_data.["comment_create_time"]
                    and set v = ()

                member i.Item
                    with get name =
                        udf_render_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IGenericPipe<_, _>) -> p.fill comment_id)
                    and set name v =
                        udf_modify_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IPipe<_>) -> p.fill (comment_id, v))
                        |> ignore }

        let aff =
            db {
                inComment
                delete "comment_id" comment_id
                whenEq 1
                execute
            }

        if aff = 1 then
            Some(comment_id, post)
        else
            None

    let gen (initBuilderItem: BuilderItem<_, _>) =
        let beforeFail =
            initBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let fail = beforeFail.fill .> panicwith

        initBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.Batch = gen finalizeBuilder.Batch
