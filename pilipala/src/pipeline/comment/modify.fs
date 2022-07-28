namespace pilipala.pipeline.comment

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module CommentModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64 * obj>>()

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

    let udf = Dict<string, IPipe<u64 * obj>>()

    do
        for KV (name, builderItem) in modifyBuilder do
            let justFail fail : IGenericPipe<_, _> = GenericPipe fail
            udf.Add(name, builderItem.fullyBuild justFail)

    member self.Body: IPipe<_> =
        modifyBuilder.Body.fullyBuild
        <| fun fail -> GenericCachePipe(set "comment_body", fail)

    member self.CreateTime: IPipe<_> =
        modifyBuilder.CreateTime.fullyBuild
        <| fun fail -> GenericCachePipe(set "comment_create_time", fail)

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
