namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.post

//TODO 考虑为Builder引入计算表达式
module IPostFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new IPostInitPipelineBuilder with
            member i.Batch = gen () }

type PostFinalizePipeline
    internal
    (
        renderBuilder: IPostRenderPipelineBuilder,
        modifyBuilder: IPostModifyPipelineBuilder,
        finalizeBuilder: IPostFinalizePipelineBuilder,
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

    let data (post_id: u64) =
        let db_data =
            db {
                inPost
                getFstRow "post_id" post_id
                execute
            }
            |> unwrap

        let post =
            { new IPost with
                member i.Id = post_id

                member i.Title
                    with get () = coerce db_data.["post_title"]
                    and set v = ()

                member i.Body
                    with get () = coerce db_data.["post_body"]
                    and set v = ()

                member i.CreateTime
                    with get () = coerce db_data.["post_create_time"]
                    and set v = ()

                member i.AccessTime
                    with get () = coerce db_data.["post_access_time"]
                    and set v = ()

                member i.ModifyTime
                    with get () = coerce db_data.["post_modify_time"]
                    and set v = ()

                member i.Item
                    with get name =
                        udf_render_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IGenericPipe<_, _>) -> p.fill post_id)
                    and set name v =
                        udf_modify_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IPipe<_>) -> p.fill (post_id, v))
                        |> ignore }

        let aff =
            db {
                inPost
                delete "post_id" post_id
                whenEq 1
                execute
            }

        if aff = 1 then
            Some(post_id, post)
        else
            None

    let gen (initBuilderItem: BuilderItem<_, _>) =
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

    member self.Batch = gen finalizeBuilder.Batch
