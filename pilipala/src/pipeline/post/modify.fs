namespace pilipala.pipeline.post

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module IPostModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dictionary<string, BuilderItem<u64 * obj>>()

        //cover/summary/view/star 交由插件实现
        { new IPostModifyPipelineBuilder with
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

type PostModifyPipeline internal (modifyBuilder: IPostModifyPipelineBuilder, db: IDbOperationBuilder) =
    let set targetKey (idVal: u64, targetVal) =
        match
            db {
                inPost
                update targetKey targetVal "post_id" idVal
                whenEq 1
                execute
            }
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (modifyBuilderItem: BuilderItem<_>) targetKey : IPipe<_> =
        let beforeFail =
            modifyBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let data = set targetKey

        let fail = beforeFail.fill .> panicwith

        modifyBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| CachePipe<_>(data, fail)

    let udf =
        Dictionary<string, IPipe<u64 * obj>>()

    do
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
                    | After after -> acc.export after
                <| CachePipe<_>(always None, fail)

            udf.Add(kv.Key, pipe)

    member self.Title =
        gen modifyBuilder.Title "post_title"

    member self.Body =
        gen modifyBuilder.Body "post_body"

    member self.CreateTime =
        gen modifyBuilder.CreateTime "post_create_time"

    member self.AccessTime =
        gen modifyBuilder.AccessTime "post_access_time"

    member self.ModifyTime =
        gen modifyBuilder.ModifyTime "post_modify_time"

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
