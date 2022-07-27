namespace pilipala.container.Comment

open dbm_test.MySql
open fsharper.op
open fsharper.op.Alias
open fsharper.typ.Pipe
open pilipala.container.comment
open pilipala.pipeline.comment


type CommentProvider
    (
        init: CommentInitPipeline,
        render: CommentRenderPipeline,
        modify: CommentModifyPipeline,
        finalize: CommentFinalizePipeline
    ) =

    member self.fetch(comment_id: u64) =
        { new IComment with
            member i.Id = comment_id

            member i.Body
                with get () = snd (render.Body.fill comment_id)
                and set v = modify.Body.fill (comment_id, v) |> ignore

            member i.CreateTime
                with get () = snd (render.CreateTime.fill comment_id)
                and set v = modify.CreateTime.fill (comment_id, v) |> ignore

            member i.Item
                with get name = fmap (fun (p: IGenericPipe<_, _>) -> p.fill comment_id |> snd) render.[name]
                and set name v =
                    fmap (fun (p: IPipe<_ * obj>) -> p.fill (comment_id, v)) modify.[name]
                    |> ignore }

    member self.create(comment: IComment) = fst (init.Batch.fill comment)
    member self.delete comment_id = finalize.Batch.fill comment_id
