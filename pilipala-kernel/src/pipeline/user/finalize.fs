namespace pilipala.pipeline.user

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline
open pilipala.access.user
open pilipala.pipeline.user

module IUserFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new IUserFinalizePipelineBuilder with
            member i.Batch = gen () }

module IUserFinalizePipeline =
    let make
        (
            renderBuilder: IUserRenderPipelineBuilder,
            finalizeBuilder: IUserFinalizePipelineBuilder,
            db: IDbOperationBuilder
        ) =

        let udf_render_no_after = //去除了After部分的udf渲染管道，因为After可能包含渲染逻辑
            let map = Dict<string, i64 -> i64 * obj>()

            for KV (name, builderItem) in renderBuilder do //遍历udf
                //udf管道初始为只会panic的GenericPipe，必须Replace后使用
                map.Add(name, builderItem.noneAfterBuild id)

            map

        let data (user_id: i64) =
            let db_data =
                db {
                    inUser
                    getFstRow "user_id" user_id
                    execute
                }
                |> unwrap

            let user = //回送被删除的用户
                { Id = user_id
                  Name = coerce db_data.["user_name"]
                  Email = coerce db_data.["user_email"]
                  CreateTime = coerce db_data.["user_create_time"]
                  AccessTime = coerce db_data.["user_create_time"]
                  Permission = coerce db_data.["user_permission"]
                  Item = //只读
                    fun name ->
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) user_id }

            let aff =
                db {
                    inUser
                    delete "user_id" user_id
                    whenEq 1
                    execute
                }

            if aff = 1 then Some user else None

        { new IUserFinalizePipeline with
            member self.Batch a =
                finalizeBuilder.Batch.fullyBuild
                <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
                |> apply a }
