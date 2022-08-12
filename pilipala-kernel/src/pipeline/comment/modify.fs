namespace pilipala.pipeline.comment

open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.comment

module ICommentModifyPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let body = gen ()
        let binding = gen ()
        let createTime = gen ()
        let userId = gen ()
        let permission = gen ()
        let udf = Dict<_, _>()

        { new ICommentModifyPipelineBuilder with
            member i.Body = body
            member i.Binding = binding
            member i.CreateTime = createTime
            member i.UserId = userId
            member i.Permission = permission

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

        let inline gen (builder: BuilderItem<_>) field =
            let set targetKey (idVal: i64, targetVal) =
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

            builder.fullyBuild
            <| fun fail x -> unwrapOr (set field x) (fun _ -> fail x)

        let body =
            gen modifyBuilder.Body "comment_body"

        let binding =
            let setBinding v =
                let (comment_id: i64), comment_binding, comment_is_reply =
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

        let createTime =
            gen modifyBuilder.CreateTime "comment_create_time"

        let userId =
            gen modifyBuilder.UserId "user_id"

        let permission =
            gen modifyBuilder.Permission "comment_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in modifyBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        { new ICommentModifyPipeline with
            member self.Body a = body a
            member self.Binding a = binding a
            member self.CreateTime a = createTime a
            member self.UserId a = userId a
            member self.Permission a = permission a
            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
