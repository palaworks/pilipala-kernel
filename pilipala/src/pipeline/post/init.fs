namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Runtime
open fsharper.op.Foldable
open pilipala.access.user
open pilipala.id
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.post

module IPostInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new IPostInitPipelineBuilder with
            member i.Batch = gen () }

type PostInitPipeline
    internal
    (
        initBuilder: IPostInitPipelineBuilder,
        palaflake: IPalaflakeGenerator,
        db: IDbOperationBuilder,
        user: IUser
    ) =
    let data (post: IPost) =
        let post_id = palaflake.next ()

        let fields: (_ * obj) list =
            [ ("post_id", post_id)
              ("post_title", post.Title)
              ("post_body", post.Body)
              ("post_create_time", post.CreateTime)
              ("post_access_time", post.AccessTime)
              ("post_modify_time", post.AccessTime)
              ("user_id", user.Id)
              ("user_permission", user.Permission) ]

        let aff =
            db {
                inPost
                insert fields
                whenEq 1
                execute
            }

        if aff = 1 then Some post_id else None

    member self.Batch =
        initBuilder.Batch.fullyBuild
        <| fun fail x -> unwrapOr (data x) (fun _ -> fail x)
