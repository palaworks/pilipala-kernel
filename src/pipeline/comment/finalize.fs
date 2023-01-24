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

        let data (comment_id: i64) =
            let db_data =
                db {
                    inComment
                    getFstRow "comment_id" comment_id
                    execute
                }
                |> unwrap

            let props =
                renderBuilder.foldl //迭代器只会遍历udf
                <| fun (map: Map<_, _>) (KV (name, it)) ->
                    //构建时去除After部分，因为After可能包含渲染逻辑和通知逻辑
                    let valueF = it.noneAfterBuild id .> snd
                    map.Add(name, valueF comment_id)
                <| Map []

            if
                db {
                    inComment
                    delete "comment_id" comment_id
                    whenEq 1
                    execute
                } = 1
            then
                { Id = comment_id
                  Body = coerce db_data.["comment_body"]
                  CreateTime = coerce db_data.["comment_create_time"]
                  ModifyTime = coerce db_data.["comment_modify_time"]
                  Binding =
                    if coerce db_data.["comment_is_reply"] then
                        BindComment(coerce db_data.["comment_binding_id"])
                    else
                        BindPost(coerce db_data.["comment_binding_id"])
                  UserId = coerce db_data.["user_id"]
                  Permission = coerce db_data.["comment_permission"]
                  Props = props }
                |> Some
            else
                None

        let batch =
            finalizeBuilder.Batch.fullyBuild
            <| fun fail id -> unwrapOrEval (data id) (fun _ -> fail id)

        { new ICommentFinalizePipeline with
            member self.Batch a = batch a }
