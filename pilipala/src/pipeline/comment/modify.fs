namespace pilipala.pipeline.comment

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

module CommentModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dictionary<string, BuilderItem<u64 * obj>>()

        { new ICommentModifyPipelineBuilder with
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


type CommentModifyPipeline internal (modifyBuilder: ICommentModifyPipelineBuilder, db: IDbOperationBuilder) =
    let set targetKey (idVal: u64, targetVal) =
        match
            db {
                inComment
                update targetKey targetVal "comment_id" idVal
                whenEq 1
                execute
            }
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (modifyBuilderItem: BuilderItem<_>) targetKey =
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

    member self.Body =
        gen modifyBuilder.Body "comment_body"

    member self.CreateTime =
        gen modifyBuilder.CreateTime "comment_create_time"

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
