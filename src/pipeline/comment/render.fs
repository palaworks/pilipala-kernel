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

module ICommentRenderPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let body = gen ()
        let binding = gen ()
        let createTime = gen ()
        let userId = gen ()
        let permission = gen ()
        let udf = Dict<_, _>() //user defined field

        //site交由插件实现
        //floor由comment_create_time推断
        { new ICommentRenderPipelineBuilder with
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

module ICommentRenderPipeline =
    let make (renderBuilder: ICommentRenderPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builder: BuilderItem<_, _>) field =
            let get targetKey (idVal: i64) =
                db {
                    inComment
                    getFstVal targetKey "comment_id" idVal
                    execute
                }
                |> fmap (fun v -> idVal, coerce v)

            builder.fullyBuild
            <| fun fail id -> unwrapOrEval (get field id) (fun _ -> fail id)

        let body =
            gen renderBuilder.Body "comment_body"

        let binding =
            let getBinding id =
                coerce
                <%> db {
                    inComment
                    getFstVal "comment_binding" "comment_id" id
                    execute
                }
                >>= fun (comment_binding: i64) ->
                        coerce
                        <%> db {
                            inComment
                            getFstVal "comment_is_reply" "comment_id" id
                            execute
                        }
                        >>= fun (comment_is_reply: bool) ->
                                if comment_is_reply then
                                    Some(id, BindComment comment_binding)
                                else
                                    Some(id, BindPost comment_binding)

            renderBuilder.Binding.fullyBuild
            <| fun fail id -> unwrapOrEval (getBinding id) (fun _ -> fail id)

        let createTime =
            gen renderBuilder.CreateTime "comment_create_time"

        let userId =
            gen renderBuilder.UserId "user_id"

        let permission =
            gen renderBuilder.Permission "comment_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in renderBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        { new ICommentRenderPipeline with
            member self.Body a = body a
            member self.Binding a = binding a
            member self.CreateTime a = createTime a
            member self.UserId a = userId a
            member self.Permission a = permission a
            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
