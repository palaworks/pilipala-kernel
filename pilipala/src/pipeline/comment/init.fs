namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Foldable
open pilipala.id
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.comment
open pilipala.container.comment

module ICommentInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentInitPipelineBuilder with
            member i.Batch = gen () }

type BindTo =
    | Post of u64
    | Comment of u64

type CommentInitPipeline
    internal
    (
        initBuilder: ICommentInitPipelineBuilder,
        palaflake: IPalaflakeGenerator,
        db: IDbOperationBuilder,
        bind_to: BindTo,
        user_id: u64
    ) =

    let data (comment: IComment) =

        let comment_id = palaflake.next ()

        let bind_id, comment_is_reply =
            match bind_to with
            | Post post_id -> post_id, false
            | Comment comment_id -> comment_id, true

        let fields: (_ * obj) list =
            [ ("comment_id", comment_id)
              ("bind_id", bind_id)
              ("user_id", user_id)
              ("comment_body", comment.Body)
              ("comment_create_time", comment.CreateTime)
              ("comment_is_reply", comment_is_reply) ]

        let aff =
            db {
                inComment
                insert fields
                whenEq 1
                execute
            }

        if aff = 1 then
            Some(comment_id, comment)
        else
            None

    member self.Batch =
        initBuilder.Batch.fullyBuild
        <| fun fail -> GenericCachePipe(data, fail)
