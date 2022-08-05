namespace pilipala.pipeline.comment

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.comment

module ICommentModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64 * obj>>()

        { new ICommentModifyPipelineBuilder with
            member i.Body = gen ()
            member i.Binding = gen ()
            member i.CreateTime = gen ()
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

module ICommentModifyPipeline =
    let make (modifyBuilder: ICommentModifyPipelineBuilder, db: IDbOperationBuilder) =
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

        let udf =
            Dict<string, u64 * obj -> u64 * obj>()

        do
            for KV (name, builderItem) in modifyBuilder do
                udf.Add(name, builderItem.fullyBuild id)

        let inline gen (builder: BuilderItem<_>) field a =
            builder.fullyBuild
            <| fun fail x -> unwrapOr (set field x) (fun _ -> fail x)
            |> apply a

        { new ICommentModifyPipeline with
            member self.Body a = gen modifyBuilder.Body "comment_body" a

            member self.Binding a =
                let setBinding v =
                    let (comment_id: u64), comment_binding, comment_is_reply =
                        match v with
                        | x, BindPost y -> x, y, false
                        | x, BindComment y -> x, y, true

                    if db {
                        inComment
                        update "comment_binding" comment_binding "comment_id" comment_id
                        whenEq 1
                        execute
                    } = 1 then
                        if db {
                            inComment
                            update "comment_is_reply" comment_is_reply "comment_id" comment_id
                            whenEq 1
                            execute
                        } = 1 then
                            Some v
                        else
                            None
                    else
                        None

                modifyBuilder.Binding.fullyBuild
                <| fun fail x -> unwrapOr (setBinding x) (fun _ -> fail x)
                |> apply a

            member self.CreateTime a =
                gen modifyBuilder.CreateTime "comment_create_time" a

            member self.UserId a = gen modifyBuilder.UserId "user_id" a

            member self.Permission a =
                gen modifyBuilder.Permission "comment_permission" a

            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
