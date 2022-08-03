namespace pilipala.pipeline.post

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
open pilipala.container.post

//TODO 考虑为Builder引入计算表达式
module IPostFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new IPostFinalizePipelineBuilder with
            member i.Batch = gen () }

type PostFinalizePipeline
    internal
    (
        renderBuilder: IPostRenderPipelineBuilder,
        modifyBuilder: IPostModifyPipelineBuilder,
        finalizeBuilder: IPostFinalizePipelineBuilder,
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

    let data (post_id: u64) =
        let db_data =
            db {
                inPost
                getFstRow "post_id" post_id
                execute
            }
            |> unwrap

        let post = //回送被删除的文章
            { new IPost with
                member i.Id = post_id

                member i.Title
                    with get () = coerce db_data.["post_title"]
                    and set v = ()

                member i.Body
                    with get () = coerce db_data.["post_body"]
                    and set v = ()

                member i.CreateTime
                    with get () = coerce db_data.["post_create_time"]
                    and set v = ()

                member i.AccessTime
                    with get () = coerce db_data.["post_access_time"]
                    and set v = ()

                member i.ModifyTime
                    with get () = coerce db_data.["post_modify_time"]
                    and set v = ()

                member i.Item
                    with get name =
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) post_id
                    and set name v =
                        udf_modify_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| apply (post_id, v)
                        |> ignore }

        let aff =
            db {
                inPost
                delete "post_id" post_id
                whenEq 1
                execute
            }

        if aff = 1 then Some post else None

    member self.Batch =
        finalizeBuilder.Batch.fullyBuild
        <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
