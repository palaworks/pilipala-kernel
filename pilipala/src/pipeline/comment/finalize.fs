namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.comment
open pilipala.container.comment

module CommentFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentInitPipelineBuilder with
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
        let map =
            Dict<string, IGenericPipe<u64, u64 * obj>>()

        for kv in renderBuilder do //遍历udf
            //视所有Replace组合为数据源
            map.Add(kv.Key, noAfterBuild (always None) kv.Value)

        map

    let udf_modify_no_after = //去除了After部分的udf修改管道，因After可能包含通知逻辑
        let map =
            Dict<string, IPipe<u64 * obj>>()

        for kv in modifyBuilder do //遍历udf
            //视所有Replace组合为数据源
            map.Add(kv.Key, noAfterBuild (always None) kv.Value)

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
                    and set v = ()

                member i.CreateTime
                    with get () = coerce db_data.["comment_create_time"]
                    and set v = ()

                member i.Item
                    with get name =
                        udf_render_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IGenericPipe<_, _>) -> p.fill comment_id)
                    and set name v =
                        udf_modify_no_after
                            .TryGetValue(name)
                            .intoOption'()
                            .fmap (fun (p: IPipe<_>) -> p.fill (comment_id, v))
                        |> ignore }

        let aff =
            db {
                inComment
                delete "comment_id" comment_id
                whenEq 1
                execute
            }

        if aff = 1 then
            Some(comment_id, comment)
        else
            None

    member self.Batch =
        fullyBuild data finalizeBuilder.Batch
