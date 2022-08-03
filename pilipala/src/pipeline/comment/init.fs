namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.access.user
open pilipala.id
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.comment
open pilipala.container.comment

module ICommentInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new ICommentInitPipelineBuilder with
            member i.Batch = gen () }

type CommentInitPipeline
    internal
    (
        initBuilder: ICommentInitPipelineBuilder,
        palaflake: IPalaflakeGenerator,
        db: IDbOperationBuilder,
        user: IUser
    ) =

    let data (comment: IComment) =

        let comment_id = palaflake.next ()

        let bind_id, comment_is_reply =
            match comment.Binding with
            | BindPost post_id -> post_id, false
            | BindComment comment_id -> comment_id, true

        let fields: (_ * obj) list =
            [ ("comment_id", comment_id)
              ("bind_id", bind_id)
              ("comment_body", comment.Body)
              ("comment_create_time", comment.CreateTime)
              ("comment_is_reply", comment_is_reply)
              ("user_id", user.Id)
              ("user_permission", user.Permission) ]

        let aff =
            db {
                inComment
                insert fields
                whenEq 1
                execute
            }

        if aff = 1 then
            Some comment_id
        else
            None

    member self.Batch =
        initBuilder.Batch.fullyBuild
        <| fun fail x -> unwrapOr (data x) (fun _ -> fail x)
