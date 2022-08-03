namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
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

type CommentFinalizePipeline
    internal
    (
        renderBuilder: ICommentRenderPipelineBuilder,
        modifyBuilder: ICommentModifyPipelineBuilder,
        finalizeBuilder: ICommentFinalizePipelineBuilder,
        db: IDbOperationBuilder
    ) =

    let udf_render_no_after = //去除了After部分的udf渲染管道，因为After可能包含渲染逻辑
        let map = Dict<string, u64 -> u64 * obj>()

        for KV (name, builderItem) in renderBuilder do //遍历udf
            //udf管道初始为只会panic的GenericPipe，必须Replace后使用
            map.Add(name, builderItem.noneAfterBuild id)

        map

    let udf_modify_no_after = //去除了After部分的udf修改管道，因After可能包含通知逻辑
        let map =
            Dict<string, u64 * obj -> u64 * obj>()

        for KV (name, builderItem) in modifyBuilder do //遍历udf
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
            { new IComment with
                member i.Id = comment_id

                member i.Body
                    with get () = coerce db_data.["comment_body"]
                    and set _ = failwith "Comment was deleted"

                member i.Binding
                    with get () =
                        if coerce db_data.["comment_is_reply"] then
                            BindComment(coerce db_data.["comment_binding"])
                        else
                            BindPost(coerce db_data.["comment_binding"])

                    and set _ = failwith "Comment was deleted"

                member i.CreateTime
                    with get () = coerce db_data.["comment_create_time"]
                    and set _ = failwith "Comment was deleted"

                member i.Item
                    with get name =
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) comment_id
                    and set name v =
                        udf_modify_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| apply (comment_id, v)
                        |> ignore }

        let aff =
            db {
                inComment
                delete "comment_id" comment_id
                whenEq 1
                execute
            }

        if aff = 1 then Some comment else None

    member self.Batch =
        finalizeBuilder.Batch.fullyBuild
        <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
