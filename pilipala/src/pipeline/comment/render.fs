namespace pilipala.pipeline.comment

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

module ICommentRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dictionary<string, BuilderItem<u64, u64 * obj>>()

        //site 交由插件实现
        //user_name交由用户组件实现（user_email）
        //email交由用户组件实现（user_email）
        //floor由comment_create_time推断
        //replies由comment_is_reply布尔决定：为true时视bind_id为回复到的comment_id
        { new ICommentRenderPipelineBuilder with
            member i.Body = gen ()
            member i.CreateTime = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

type CommentRenderPipeline internal (renderBuilder: ICommentRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get targetKey (idVal: u64) =
        db {
            inComment
            getFstVal targetKey "comment_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let gen (renderBuilderItem: BuilderItem<_, _>) targetKey =
        let data = get targetKey

        let fail =
            (renderBuilderItem.beforeFail.foldr
                (fun p (acc: IGenericPipe<_, _>) -> acc.export p)
                (GenericPipe<_, _>(id)))
                .fill //before fail
            .> panicwith

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

    member self.Body =
        gen renderBuilder.Body "comment_body"

    member self.CreateTime =
        gen renderBuilder.CreateTime "comment_create_time"

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
