namespace pilipala.pipeline.user

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
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

type UserFinalizePipeline
    internal
    (
        renderBuilder: IUserRenderPipelineBuilder,
        modifyBuilder: IUserModifyPipelineBuilder,
        finalizeBuilder: IUserFinalizePipelineBuilder,
        db: IDbOperationBuilder,
        ug: IUser
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

    let data (user_id: u64) =
        let db_data =
            db {
                inUser
                getFstRow "user_id" user_id
                execute
            }
            |> unwrap

        let user = //回送被删除的文章
            { new IUser with
                member i.Id = user_id

                member i.Name
                    with get () = coerce db_data.["user_name"]
                    and set v = ()

                member i.Email
                    with get () = coerce db_data.["user_email"]
                    and set v = ()

                member i.Permission
                    with get () = coerce db_data.["user_permission"]
                    and set v = ()

                member i.CreateTime
                    with get () = coerce db_data.["user_create_time"]
                    and set v = ()

                member i.AccessTime
                    with get () = coerce db_data.["user_create_time"]
                    and set v = ()

                member i.Item
                    with get name =
                        udf_render_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| (apply ..> snd) user_id
                    and set name v =
                        udf_modify_no_after.TryGetValue(name).intoOption'()
                            .fmap
                        <| apply (user_id, v)
                        |> ignore }

        let aff =
            db {
                inUser
                delete "user_id" user_id
                whenEq 1
                execute
            }

        if aff = 1 then
            Some(user_id, user)
        else
            None

    member self.Batch =
        finalizeBuilder.Batch.fullyBuild
        <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)
