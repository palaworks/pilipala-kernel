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
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new ICommentFinalizePipelineBuilder with
            member i.Batch = gen () }

module ICommentFinalizePipeline =
    let make
        (
            renderBuilder: ICommentRenderPipelineBuilder,
            finalizeBuilder: ICommentFinalizePipelineBuilder,
            db: IDbOperationBuilder
        ) =

        let udf_render_no_after = //去除了After部分的udf渲染管道，因为After可能包含渲染逻辑
            let map = Dict<string, u64 -> u64 * obj>()

            for KV (name, builderItem) in renderBuilder do //遍历udf
                //udf管道初始为只会panic的GenericPipe，必须Replace后使用
                map.Add(name, builderItem.noneAfterBuild id)

            map

        let data (comment_id: u64) =
            let db_data =
                db {
                    inComment
                    getFstRow "comment_id" comment_id
                    execute
                }
                |> unwrap

            let comment = //回送被删除的评论
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

            let aff =
                db {
                    inComment
                    delete "comment_id" comment_id
                    whenEq 1
                    execute
                }

            if aff = 1 then Some comment else None

        { new ICommentFinalizePipeline with
            member self.Batch a =
                finalizeBuilder.Batch.fullyBuild
                <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
                |> apply a }
