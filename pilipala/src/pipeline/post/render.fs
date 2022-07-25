namespace pilipala.pipeline.post

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module IPostRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dictionary<string, BuilderItem<u64, u64 * obj>>()

        //cover/summary/view/star 交由插件实现
        { new IPostRenderPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }


type PostRenderPipeline internal (renderBuilder: IPostRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get target (idVal: u64) =
        db {
            inPost
            getFstVal target "post_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let gen (renderBuilderItem: BuilderItem<_, _>) targetKey =
        let beforeFail =
            renderBuilderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let data = get targetKey

        let fail = beforeFail.fill .> panicwith

        renderBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    let udf =
        Dictionary<string, IGenericPipe<u64, u64 * obj>>()

    do
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
                    | Replace f -> f acc
                    | After after -> acc.export after
                <| GenericCachePipe<_, _>(always None, fail)

            udf.Add(kv.Key, pipe)

    member self.Title =
        gen renderBuilder.Title "post_title"

    member self.Body =
        gen renderBuilder.Body "post_body"

    member self.CreateTime =
        gen renderBuilder.CreateTime "post_create_time"

    member self.AccessTime =
        gen renderBuilder.AccessTime "post_access_time"

    member self.ModifyTime =
        gen renderBuilder.ModifyTime "post_modify_time"

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
