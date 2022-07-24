namespace pilipala.container.Comment

open fsharper.op.Alias
open pilipala.container.comment
open pilipala.pipeline.comment


type CommentProvider(render: CommentRenderPipeline, modify: CommentModifyPipeline, init: CommentInitPipeline) =

    (*
            member i.UserName
                with get () = snd (render.UserName.fill comment_id)
                and set v = modify.Body.fill (comment_id, v) |> ignore
    *)
    member self.fetch(comment_id: u64) =
        { new IComment with
            member i.Id = comment_id

            member i.Body
                with get () = snd (render.Body.fill comment_id)
                and set v = modify.Body.fill (comment_id, v) |> ignore

            member i.CreateTime
                with get () = snd (render.CreateTime.fill comment_id)
                and set v = modify.CreateTime.fill (comment_id, v) |> ignore }

    member self.create(comment: IComment) = fst (init.Batch.fill comment)
