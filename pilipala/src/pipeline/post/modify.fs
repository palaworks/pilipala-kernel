namespace pilipala.pipeline.post

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

module IPostModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64 * obj>>()

        { new IPostModifyPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen ()
            member i.UserId = gen ()
            member i.Permission = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

module IPostModifyPipeline =
    let make (modifyBuilder: IPostModifyPipelineBuilder, db: IDbOperationBuilder) =
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

        let udf =
            Dict<string, u64 * obj -> u64 * obj>()

        do
            for KV (name, builderItem) in modifyBuilder do
                udf.Add(name, builderItem.fullyBuild id)

        let inline gen (builder: BuilderItem<_>) field a =
            builder.fullyBuild
            <| fun fail x -> unwrapOr (set field x) (fun _ -> fail x)
            |> apply a

        { new IPostModifyPipeline with
            member i.Title a = gen modifyBuilder.Title "post_title" a

            member i.Body a = gen modifyBuilder.Body "post_body" a

            member i.CreateTime a =
                gen modifyBuilder.CreateTime "post_create_time" a

            member i.AccessTime a =
                gen modifyBuilder.AccessTime "post_access_time" a

            member i.ModifyTime a =
                gen modifyBuilder.ModifyTime "post_modify_time" a

            member i.UserId a = gen modifyBuilder.UserId "user_id" a

            member i.Permission a =
                gen modifyBuilder.Permission "post_permission" a

            member i.Item(name: string) = udf.TryGetValue(name).intoOption' () }
