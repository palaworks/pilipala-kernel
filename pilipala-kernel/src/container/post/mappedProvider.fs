module pilipala.container.post.IMappedPostProvider

open fsharper.op
open fsharper.typ
open fsharper.op.Pattern
open pilipala.pipeline.post

let make
    (
        init: IPostInitPipeline,
        render: IPostRenderPipeline,
        modify: IPostModifyPipeline,
        finalize: IPostFinalizePipeline
    ) =
    { new IMappedPostProvider with
        member self.fetch post_id =
            { new IMappedPost with
                member i.Id = post_id

                member i.Title
                    with get () = snd (render.Title post_id)
                    and set v = modify.Title(post_id, v) |> ignore

                member i.Body
                    with get () = snd (render.Body post_id)
                    and set v = modify.Body(post_id, v) |> ignore

                member i.CreateTime
                    with get () = snd (render.CreateTime post_id)
                    and set v = modify.CreateTime(post_id, v) |> ignore

                member i.AccessTime
                    with get () = snd (render.AccessTime post_id)
                    and set v = modify.AccessTime(post_id, v) |> ignore

                member i.ModifyTime
                    with get () = snd (render.ModifyTime post_id)
                    and set v = modify.ModifyTime(post_id, v) |> ignore

                member i.UserId
                    with get () = snd (render.UserId post_id)
                    and set v = modify.UserId(post_id, v) |> ignore

                member i.Permission
                    with get () = snd (render.Permission post_id)
                    and set v = modify.Permission(post_id, v) |> ignore

                member i.Item
                    with get name = fmap ((apply ..> snd) post_id) render.[name]
                    and set name v =
                        bind
                        <| v
                        <| fun v -> fmap (apply (post_id, v)) modify.[name]
                        |> ignore }

        member self.create post =
            self.fetch (init.Batch post)
            |> effect (fun mapped -> //初始化udf
                for KV (name, value) in post do
                    mapped.[name] <- Some value)

        member self.delete post_id = finalize.Batch post_id }
