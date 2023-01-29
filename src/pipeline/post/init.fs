namespace pilipala.pipeline.post

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.post

module IPostInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let batch = gen ()

        { new IPostInitPipelineBuilder with
            member i.Batch = batch }

module IPostInitPipeline =
    let make (initBuilder: IPostInitPipelineBuilder, db: IDbOperationBuilder) =
        let data (post: PostData) =
            let fields: (_ * obj) list =
                [ ("post_id", post.Id)
                  ("post_title", post.Title)
                  ("post_body", post.Body)
                  ("post_create_time", post.CreateTime)
                  ("post_access_time", post.AccessTime)
                  ("post_modify_time", post.AccessTime)
                  ("post_permission", post.Permission)
                  ("user_id", post.UserId) ]

            let aff =
                db {
                    inPost
                    insert fields
                    whenEq 1
                    execute
                }

            if aff = 1 then Some post.Id else None

        let batch =
            initBuilder.Batch.fullyBuild
            <| fun fail x -> unwrapOrEval (data x) (fun _ -> fail x)

        { new IPostInitPipeline with
            member i.Batch a = batch a }
