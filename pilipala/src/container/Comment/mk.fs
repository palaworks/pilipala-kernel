﻿namespace pilipala.container.Comment

open System
open dbm_test.MySql
open fsharper.op
open fsharper.typ.Pipe
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.container.comment
open pilipala.pipeline.comment


type CommentRecordProvider(render: CommentRenderPipeline, modify: CommentModifyPipeline, init: CommentInitPipeline) =

    member self.fetch(comment_id: u64) =
        { new IComment with
            member i.Body
                with get () = snd (render.Body.fill comment_id)
                and set v = modify.Body.fill (comment_id, v) |> ignore }

    member self.create(comment: IComment) = fst (init.Batch.fill comment)
(*
            /// 评论id
            member self.commentId = commentId
            member self.ownerMetaId: u64 = cache.get "ownerMetaId"

            member self.ownerMetaId
                with set (v: u64) = cache.set "ownerMetaId" v

            member self.replyTo: u64 = cache.get "replyTo"

            member self.replyTo
                with set (v: u64) = cache.set "replyTo" v

            member self.nick: string = cache.get "nick"

            member self.nick
                with set (v: string) = cache.set "nick" v

            member self.content: string = cache.get "content"

            member self.content
                with set (v: string) = cache.set "content" v

            member self.email: string = cache.get "email"

            member self.email
                with set (v: string) = cache.set "email" v

            member self.site: string = cache.get "site"

            member self.site
                with set (v: string) = cache.set "site" v

            member self.ctime: DateTime = cache.get "ctime"

            member self.ctime
                with set (v: DateTime) = cache.set "ctime" v
*)
