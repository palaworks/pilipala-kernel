﻿module pilipala.container.comment.IMappedCommentProvider

open fsharper.op
open fsharper.typ
open fsharper.op.Pattern
open pilipala.container.comment
open pilipala.pipeline.comment

let make
    (
        init: ICommentInitPipeline,
        render: ICommentRenderPipeline,
        modify: ICommentModifyPipeline,
        finalize: ICommentFinalizePipeline
    ) =
    { new IMappedCommentProvider with
        member self.fetch comment_id =
            { new IMappedComment with
                member i.Id = comment_id

                member i.Body
                    with get () = snd (render.Body comment_id)
                    and set v = modify.Body(comment_id, v) |> ignore

                member i.CreateTime
                    with get () = snd (render.CreateTime comment_id)
                    and set v = modify.CreateTime(comment_id, v) |> ignore

                member i.ModifyTime
                    with get () = snd (render.ModifyTime comment_id)
                    and set v = modify.ModifyTime(comment_id, v) |> ignore

                member i.Binding
                    with get () = snd (render.Binding comment_id)
                    and set v = modify.Binding(comment_id, v) |> ignore

                member i.UserId
                    with get () = snd (render.UserId comment_id)
                    and set v = modify.UserId(comment_id, v) |> ignore

                member i.Permission
                    with get () = snd (render.Permission comment_id)
                    and set v = modify.Permission(comment_id, v) |> ignore

                member i.Item
                    with get name = fmap ((apply ..> snd) comment_id) render.[name]
                    and set name v = bind <| v <| (fun v -> fmap (apply (comment_id, v)) modify.[name]) |> ignore }

        member self.create comment =
            self.fetch (init.Batch comment)
            |> effect (fun mapped -> //初始化udf
                for KV (name, value) in comment do
                    mapped.[name] <- Some value)

        member self.delete comment_id = finalize.Batch comment_id }
