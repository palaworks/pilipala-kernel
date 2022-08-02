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
        db: IDbOperationBuilder,
        loginUser: IUser
    ) =

    member self.GetPost id =
        if db {
            inPost
            getFstVal "post_id" "post_id" id
            execute
        } = None then
            failwith "Invalid post id"
        else
            let postUserPermission =
                db {
                    inPost
                    getFstVal "user_permission" "post_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let postUserId =
                db {
                    inPost
                    getFstVal "user_id" "post_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let targetPost = postProvider.fetch id

            let inline genGetter getter =
                if loginUser.CanReadPost(postUserPermission, postUserId) then
                    getter ()
                else
                    failwith "Permission denied"

            let inline genSetter setter =
                if loginUser.CanWritePost(postUserPermission, postUserId) then
                    setter |> ignore
                else
                    failwith "Permission denied"

            { new IPost with
                member i.Id = id

                member i.Title
                    with get () = genGetter (fun _ -> targetPost.Title)
                    and set v = genSetter (fun v -> targetPost.Title <- v)

                member i.Body
                    with get () = genGetter (fun _ -> targetPost.Body)
                    and set v = genSetter (fun v -> targetPost.Body <- v)

                member i.CreateTime
                    with get () = genGetter (fun _ -> targetPost.CreateTime)
                    and set v = genSetter (fun v -> targetPost.CreateTime <- v)

                member i.AccessTime
                    with get () = genGetter (fun _ -> targetPost.AccessTime)
                    and set v = genSetter (fun v -> targetPost.AccessTime <- v)

                member i.ModifyTime
                    with get () = genGetter (fun _ -> targetPost.ModifyTime)
                    and set v = genSetter (fun v -> targetPost.ModifyTime <- v)

                member i.Item
                    with get name = genGetter (fun _ -> targetPost.[name])
                    and set name v = genSetter (fun v -> targetPost.[name] <- v) }

    member self.NewPost post =
        let _, post = postProvider.create post
        post

    member self.GetComment id =
        if db {
            inPost
            getFstVal "comment_id" "comment_id" id
            execute
        } = None then
            failwith "Invalid comment id"
        else
            let commentUserPermission =
                db {
                    inComment
                    getFstVal "user_permission" "comment_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let commentUserId =
                db {
                    inComment
                    getFstVal "user_id" "comment_id" id
                    execute
                }
                |> unwrap
                |> coerce

            let targetComment = commentProvider.fetch id

            let inline genGetter getter =
                if loginUser.CanReadComment(commentUserPermission, commentUserId) then
                    getter ()
                else
                    failwith "Permission denied"

            let inline genSetter setter =
                if loginUser.CanWriteComment(commentUserPermission, commentUserId) then
                    setter |> ignore
                else
                    failwith "Permission denied"

            { new IComment with
                member i.Id = id

                member i.Body
                    with get () = genGetter (fun _ -> targetComment.Body)
                    and set v = genSetter (fun v -> targetComment.Body <- v)

                member i.Binding
                    with get () = genGetter (fun _ -> targetComment.Binding)
                    and set v = genSetter (fun v -> targetComment.Binding <- v)

                member i.CreateTime
                    with get () = genGetter (fun _ -> targetComment.CreateTime)
                    and set v = genSetter (fun v -> targetComment.CreateTime <- v)

                member i.Item
                    with get name = genGetter (fun _ -> targetComment.[name])
                    and set name v = genSetter (fun v -> targetComment.[name] <- v) }

    member self.NewComment post =
        let _, comment = commentProvider.create post
        comment

    member self.GetUser id =
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

                member i.Permission
                    with get () = genGetter (fun _ -> targetUser.Permission)
                    and set v = genSetter (fun v -> targetUser.Permission <- v)

                member i.CreateTime
                    with get () = genGetter (fun _ -> targetUser.CreateTime)
                    and set v = genSetter (fun v -> targetUser.CreateTime <- v)

                member i.AccessTime
                    with get () = genGetter (fun _ -> targetUser.AccessTime)
                    and set v = genSetter (fun v -> targetUser.AccessTime <- v)

                member i.Item
                    with get name = genGetter (fun _ -> targetUser.[name])
                    and set name v = genSetter (fun v -> targetUser.[name] <- v) }

    member self.NewUser user =
        let _, user = userProvider.create user
        user
