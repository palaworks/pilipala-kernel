namespace pilipala.pipeline.comment

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.comment
open pilipala.container.comment

module ICommentFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let batch = gen ()

        { new ICommentFinalizePipelineBuilder with
            member i.Batch = batch }

module ICommentFinalizePipeline =
    let make
        (
            renderBuilder: ICommentRenderPipelineBuilder,
            finalizeBuilder: ICommentFinalizePipelineBuilder,
            db: IDbOperationBuilder
        ) =

        let udf_render_no_after = //去除了After部分的udf渲染管道，因为After可能包含渲染逻辑
            let map = Dict<string, i64 -> i64 * obj>()

            for KV (name, builderItem) in renderBuilder do //遍历udf
                //udf管道初始为只会panic的GenericPipe，必须Replace后使用
                map.Add(name, builderItem.noneAfterBuild id)

            map

        let data (comment_id: i64) =
            let db_data =
                db {
                    inComment
                    getFstRow "comment_id" comment_id
                    execute
                }
                |> unwrap

            if db {
                inComment
                delete "comment_id" comment_id
                whenEq 1
                execute
            } = 1 then
                { Id = comment_id
                  Body = coerce db_data.["comment_body"]

                  Binding =
                      if coerce db_data.["comment_is_reply"] then
                          BindComment(coerce db_data.["comment_binding"])
                      else
                          BindPost(coerce db_data.["comment_binding"])

                  CreateTime = coerce db_data.["comment_create_time"]
                  UserId = coerce db_data.["user_id"]
                  Permission = coerce db_data.["comment_permission"]
                  Item =
                    fun name -> //只读
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) comment_id }
                |> Some
            else
                None

        let batch =
            finalizeBuilder.Batch.fullyBuild
            <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)

        { new ICommentFinalizePipeline with
            member self.Batch a = batch a }
