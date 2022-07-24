namespace pilipala.container.post

open fsharper.op.Alias
open pilipala.pipeline.post

type PostProvider(render: PostRenderPipeline, modify: PostModifyPipeline, init: PostInitPipeline) =

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
                and set v = modify.ModifyTime.fill (post_id, v) |> ignore }

    member self.create(post: IPost) = fst (init.Batch.fill post)
