namespace pilipala.pipeline.user

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.access.user
open pilipala.pipeline.user

module IUserFinalizePipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let batch = gen ()

        { new IUserFinalizePipelineBuilder with
            member i.Batch = batch }

module IUserFinalizePipeline =
    let make
        (
            renderBuilder: IUserRenderPipelineBuilder,
            finalizeBuilder: IUserFinalizePipelineBuilder,
            db: IDbOperationBuilder
        ) =

        let data (user_id: i64) =
            let db_data =
                db {
                    inUser
                    getFstRow "user_id" user_id
                    execute
                }
                |> unwrap

            let props =
                renderBuilder.foldl //迭代器只会遍历udf
                <| fun (map: Map<_, _>) (KV (name, it)) ->
                    //构建时去除After部分，因为After可能包含渲染逻辑和通知逻辑
                    let valueF = it.noneAfterBuild id .> snd
                    map.Add(name, valueF user_id)
                <| Map []

            if db {
                inUser
                delete "user_id" user_id
                whenEq 1
                execute
            } = 1 then
                { Id = user_id
                  Name = coerce db_data.["user_name"]
                  Email = coerce db_data.["user_email"]
                  CreateTime = coerce db_data.["user_create_time"]
                  AccessTime = coerce db_data.["user_create_time"]
                  Permission = coerce db_data.["user_permission"]
                  Props = props }
                |> Some
            else
                None

        let batch =
            finalizeBuilder.Batch.fullyBuild
            <| fun fail id -> unwrapOr (data id) (fun _ -> fail id)

        { new IUserFinalizePipeline with
            member self.Batch a = batch a }
