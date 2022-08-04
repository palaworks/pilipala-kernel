namespace pilipala

open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment
open pilipala.data.db

type Pilipala
    internal
    (
        postProvider: IPostProvider,
        commentProvider: ICommentProvider,
        userProvider: IUserProvider,
        db: IDbOperationBuilder
    ) =


    member self.LoginWith user =
        if db {
            inPost
            getFstVal "user_id" "user_id" id
            execute
        } = None then
            failwith "Invalid user id"
        else
            let userUserPermission =
                db {
                    inUser
                    getFstVal "user_permission" "user_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let userUserId =
                db {
                    inUser
                    getFstVal "user_id" "user_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let targetUser = userProvider.fetch id

            let inline genGetter getter =
                if loginUser.CanReadUser(userUserPermission, userUserId) then
                    getter ()
                else
                    failwith "Permission denied"

            let inline genSetter setter =
                if loginUser.CanWriteUser(userUserPermission, userUserId) then
                    setter |> ignore
                else
                    failwith "Permission denied"

            { new IUser with
                member i.Id = id

                member i.Name
                    with get () = genGetter (fun _ -> targetUser.Name)
                    and set v = genSetter (fun v -> targetUser.Name <- v)

                member i.Email
                    with get () = genGetter (fun _ -> targetUser.Email)
                    and set v = genSetter (fun v -> targetUser.Email <- v)

                member i.Permission =
                    genGetter (fun _ -> targetUser.Permission)

                member i.CreateTime
                    with get () = genGetter (fun _ -> targetUser.CreateTime)
                    and set v = genSetter (fun v -> targetUser.CreateTime <- v)

                member i.AccessTime
                    with get () = genGetter (fun _ -> targetUser.AccessTime)
                    and set v = genSetter (fun v -> targetUser.AccessTime <- v)

                member i.Item
                    with get name = genGetter (fun _ -> targetUser.[name])
                    and set name v = genSetter (fun v -> targetUser.[name] <- v) }
