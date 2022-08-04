module pilipala.access.user.IMappedUserProvider

open System
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open pilipala.access.user
open pilipala.container.comment
open pilipala.container.post
open pilipala.pipeline.user

let make
    (
        init: UserInitPipeline,
        render: UserRenderPipeline,
        modify: UserModifyPipeline,
        finalize: UserFinalizePipeline
    ) =
    { new IMappedUserProvider with
        member self.fetch user_id =
            { new IMappedUser with
                member i.Id = user_id

                member i.Name
                    with get () = snd (render.Name user_id)
                    and set v = modify.Name(user_id, v) |> ignore

                member i.Email
                    with get () = snd (render.Email user_id)
                    and set v = modify.Email(user_id, v) |> ignore

                member i.CreateTime
                    with get () = snd (render.CreateTime user_id)
                    and set v = modify.CreateTime(user_id, v) |> ignore

                member i.AccessTime
                    with get () = snd (render.AccessTime user_id)
                    and set v = modify.AccessTime(user_id, v) |> ignore

                member i.Permission
                    with get () = snd (render.Permission user_id)
                    and set v = modify.Permission(user_id, v) |> ignore

                member i.Item
                    with get name = fmap ((apply ..> snd) user_id) render.[name]
                    and set name v =
                        bind
                        <| v
                        <| fun v -> fmap (apply (user_id, v)) modify.[name]
                        |> ignore }

        member self.create user = self.fetch (init.Batch user)
        member self.delete user_id = finalize.Batch user_id }

//member i.GetPost()=
(*
                    member i.NewPost(title, body) =
                        if i.Permission &&& 300us <> 0us then
                            let newPost =
                                { Id = 0UL
                                  Title = title
                                  Body = body
                                  CreateTime = DateTime.Now
                                  AccessTime = DateTime.Now
                                  ModifyTime = DateTime.Now
                                  Permission = i.Permission
                                  Item = always None }

                            //应有验证
                            Ok(postProvider.create newPost)
                        else
                            Err "Permission denied"

                    member i.NewCommentOn(post: IPost, body: string) =
                        if (i.Id = post.UserId)
                           || ((i.Permission &&& 48us) > (post.Permission &&& 48us)) then
                            let newComment =
                                { Id = 0UL
                                  Body = body
                                  CreateTime = DateTime.Now
                                  Binding = BindPost post.Id
                                  Permission = i.Permission
                                  Item = always None }

                            //应有验证
                            Ok(commentProvider.create newComment)
                        else
                            Err "Permission denied"

                    member i.NewCommentOn(comment: IComment, body: string) =
                        if (i.Id = comment.UserId)
                           || ((i.Permission &&& 48us) > (comment.Permission &&& 48us)) then
                            let newComment =
                                { Id = 0UL
                                  Body = body
                                  CreateTime = DateTime.Now
                                  Binding = BindComment comment.Id
                                  Permission = i.Permission
                                  Item = always None }

                            //应有验证
                            Ok(commentProvider.create newComment)
                        else
                            Err "Permission denied" }

            member self.create user = self.fetch (init.Batch user)
            member self.delete user_id = finalize.Batch user_id *)
