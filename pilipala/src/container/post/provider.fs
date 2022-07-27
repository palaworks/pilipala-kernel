namespace pilipala.container.post

open fsharper.typ
open fsharper.op
open fsharper.op.Alias
open fsharper.typ.Pipe
open pilipala.pipeline.post

type PostProvider
    (
        init: PostInitPipeline,
        render: PostRenderPipeline,
        modify: PostModifyPipeline,
        finalize: PostFinalizePipeline
    ) =

    member self.fetch(post_id: u64) =
        { new IPost with
            member i.Id = post_id

            member i.Title
                with get () = snd (render.Title.fill post_id)
                and set v = modify.Title.fill (post_id, v) |> ignore

            member i.Body
                with get () = snd (render.Body.fill post_id)
                and set v = modify.Body.fill (post_id, v) |> ignore

            member i.CreateTime
                with get () = snd (render.CreateTime.fill post_id)
                and set v = modify.CreateTime.fill (post_id, v) |> ignore

            member i.AccessTime
                with get () = snd (render.AccessTime.fill post_id)
                and set v = modify.AccessTime.fill (post_id, v) |> ignore

            member i.ModifyTime
                with get () = snd (render.ModifyTime.fill post_id)
                and set v = modify.ModifyTime.fill (post_id, v) |> ignore

            member i.Item
                with get name = fmap (fun (p: IGenericPipe<_, _>) -> p.fill post_id |> snd) render.[name]
                and set name v =
                    fmap (fun (p: IPipe<_ * obj>) -> p.fill (post_id, v)) modify.[name]
                    |> ignore }

    member self.create(post: IPost) = fst (init.Batch.fill post)
    member self.delete post_id = finalize.Batch.fill post_id
