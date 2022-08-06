namespace pilipala.pipeline.post

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
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

module IPostFinalizePipeline =
    let make
        (
            renderBuilder: IPostRenderPipelineBuilder,
            finalizeBuilder: IPostFinalizePipelineBuilder,
            db: IDbOperationBuilder
        ) =

        let udf_render_no_after = //去除了After部分的udf渲染管道，因为After可能包含渲染逻辑
            let map = Dict<string, u64 -> u64 * obj>()

            for KV (name, builderItem) in renderBuilder do //遍历udf
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
                { Id = post_id
                  Title = coerce db_data.["post_title"]
                  Body = coerce db_data.["post_body"]
                  CreateTime = coerce db_data.["post_create_time"]
                  AccessTime = coerce db_data.["post_access_time"]
                  ModifyTime = coerce db_data.["post_modify_time"]
                  UserId = coerce db_data.["user_id"]
                  Permission = coerce db_data.["post_permission"]
                  Item = //只读
                    fun name ->
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) post_id }

            let aff =
                db {
                    inPost
                    delete "post_id" post_id
                    whenEq 1
                    execute
                }

            if aff = 1 then Some post else None

        { new IPostFinalizePipeline with
            member i.Batch a =
                finalizeBuilder.Batch.fullyBuild
                <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
                |> apply a }
