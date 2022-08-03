namespace pilipala.access.user

open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open pilipala.access.user
open pilipala.pipeline.user

module IUserProvider =

    let make
        (
            init: UserInitPipeline,
            render: UserRenderPipeline,
            modify: UserModifyPipeline,
            finalize: UserFinalizePipeline
        ) =
        { new IUserProvider with
            member self.fetch user_id =
                { new IUser with
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

                    member i.Item
                        with get name = fmap ((apply ..> snd) user_id) render.[name]
                        and set name v =
                            bind
                            <| v
                            <| fun v -> fmap (apply (user_id, v)) modify.[name]
                            |> ignore }

            member self.create user = self.fetch (init.Batch user)
            member self.delete user_id = finalize.Batch user_id }
