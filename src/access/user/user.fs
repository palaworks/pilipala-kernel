namespace pilipala.access.user

open System
open Microsoft.Extensions.Logging
open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.data.db
open pilipala.util.log
open pilipala.util.hash
open pilipala.container.post
open pilipala.container.comment

type internal User
    (
        palaflake: IPalaflakeGenerator,
        mappedPostProvider: IMappedPostProvider,
        mappedCommentProvider: IMappedCommentProvider,
        mappedUserProvider: IMappedUserProvider,
        handler: IMappedUser,
        mapped: IMappedUser,
        db: IDbOperationBuilder,
        postLogger: ILogger<Post>,
        commentLogger: ILogger<Comment>,
        userLogger: ILogger<User>
    ) as impl =
    member self.Id = mapped.Id

    member self.ReadPermissionLv = mapped.Permission &&& 48us >>> 4
    member self.WritePermissionLv = mapped.Permission &&& 12us >>> 2
    member self.CommentPermissionLv = mapped.Permission &&& 3us
    member self.ReadUserPermissionLv = mapped.Permission &&& 768us >>> 8
    member self.WriteUserPermissionLv = mapped.Permission &&& 192us >>> 6

    member self.Name = mapped.Name
    member self.Email = mapped.Email
    member self.CreateTime = mapped.CreateTime
    member self.AccessTime = mapped.AccessTime
    member self.Permission = mapped.Permission

    member self.Item name = mapped.[name]

    member self.NewPost title body =
        if self.WritePermissionLv <> 0us then
            { Id = palaflake.next ()
              Title = title
              Body = body
              CreateTime = DateTime.Now
              AccessTime = DateTime.Now
              ModifyTime = DateTime.Now
              UserId = mapped.Id
              Permission =
                let r = 00uy //可见性默认为00

                r
                ||| u8 (mapped.Permission &&& 12us) //从用户继承的修改权
                ||| r //可评论性与可见性默认相同
              Props = Map [] }
            |> mappedPostProvider.create
            |> fun x ->
                Post(palaflake, x, mappedPostProvider, mappedCommentProvider, db, mapped, postLogger, commentLogger)
                :> IPost
            |> Ok
        else
            userLogger.error "Create Post Failed: Permission denied" |> Err

    member self.GetPost id =
        if
            db {
                inPost
                getFstVal "post_id" "post_id" id
                execute
            } = None
        then
            userLogger.error $"Get Post Failed: Invalid post id({id})" |> Err
        else
            Post(
                palaflake,
                mappedPostProvider.fetch id,
                mappedPostProvider,
                mappedCommentProvider,
                db,
                mapped,
                postLogger,
                commentLogger
            )
            :> IPost
            |> Ok

    member self.GetComment id =
        if
            db {
                inComment
                getFstVal "comment_id" "comment_id" id
                execute
            } = None
        then
            userLogger.error $"Get Comment Failed: Invalid comment id({id})" |> Err
        else
            Comment(palaflake, mappedCommentProvider.fetch id, mappedCommentProvider, db, mapped, commentLogger)
            :> IComment
            |> Ok

    member self.NewUser name (pwd: string) permission =
        let creator_wu_lv = self.Permission &&& 192us >>> 6
        let target_ru_lv = permission &&& 768us >>> 8
        let target_wu_lv = permission &&& 192us >>> 6
        let target_r_lv = permission &&& 48us >>> 4
        let target_w_lv = permission &&& 12us >>> 2
        let target_c_lv = permission &&& 3us

        //小于创建者的权限级别（防止管理员自克隆）
        if
            target_ru_lv >= creator_wu_lv
            || target_wu_lv >= creator_wu_lv
            || target_r_lv >= creator_wu_lv
            || target_w_lv >= creator_wu_lv
            || target_c_lv >= creator_wu_lv
        then
            $"Operation {nameof self.NewUser} Failed: illegal permission({permission}) \
              (any target permission({permission}) must be lower than creator({self.Name})'s write user permission)"
            |> userLogger.error
            |> Err
        //保证可见性>=可评性>=可写性
        elif target_r_lv < target_c_lv then
            $"Operation {nameof self.NewUser} Failed: illegal permission({permission}) \
              (violate constraint: read level >= comment level)"
            |> userLogger.error
            |> Err
        elif target_c_lv < target_w_lv then
            $"Operation {nameof self.NewUser} Failed: illegal permission({permission}) \
              (violate constraint: comment level >= write level)"
            |> userLogger.error
            |> Err
        //仅限pl_register(wu级别2)及root(wu级别3)创建用户
        elif self.WriteUserPermissionLv >= 2us then
            if
                db {
                    inUser
                    getFstVal "user_name" "user_name" name
                    execute
                }
                <> None
            then
                userLogger.error $"Operation {nameof self.NewUser} Failed: username({name}) already exists"
                |> Err
            else
                { Id = palaflake.next ()
                  Name = name
                  Email = "" //应由用户自行指定
                  CreateTime = DateTime.Now
                  AccessTime = DateTime.Now
                  Permission = permission
                  Props = Map [] } //应由用户自行指定
                |> mappedUserProvider.create
                |> fun x ->
                    let aff =
                        db {
                            inUser
                            update "user_pwd_hash" pwd.bcrypt "user_id" x.Id
                            whenEq 1
                            execute
                        }

                    if aff <> 1 then //非期望行为，let it crash
                        userLogger.error
                            $"Operation {nameof self.NewUser} Failed: unable to initialize user pwd (affected:{aff})"
                        |> failwith

                    User(
                        palaflake,
                        mappedPostProvider,
                        mappedCommentProvider,
                        mappedUserProvider,
                        handler,
                        x,
                        db,
                        postLogger,
                        commentLogger,
                        userLogger
                    )
                    :> IUser
                |> Ok
        else
            userLogger.error $"Operation {nameof self.NewUser} Failed: Permission denied"
            |> Err

    member self.GetUser id =
        if self.ReadUserPermissionLv >= 2us then //TODO，暂不作实现，仅限pl_register(ru级别2)及root(ru级别3)访问
            if
                db {
                    inUser
                    getFstVal "user_id" "user_id" id
                    execute
                } = None
            then
                userLogger.error $"Operation {nameof self.GetUser} Failed: Invalid user id({id})"
                |> Err
            else
                User(
                    palaflake,
                    mappedPostProvider,
                    mappedCommentProvider,
                    mappedUserProvider,
                    handler,
                    mappedUserProvider.fetch id,
                    db,
                    postLogger,
                    commentLogger,
                    userLogger
                )
                :> IUser
                |> Ok
        else
            userLogger.error $"Operation {nameof self.GetUser} Failed: Permission denied (target user id: {id})"
            |> Err

    member inline private self.GetPostGen(mask: u8) =
        Seq.unfold
        <| fun list ->
            match list with
            | x :: xs ->
                let post =
                    Post(
                        palaflake,
                        mappedPostProvider.fetch (coerce x),
                        mappedPostProvider,
                        mappedCommentProvider,
                        db,
                        mapped,
                        postLogger,
                        commentLogger
                    )
                    :> IPost

                Option.Some(post, xs)
            | _ -> Option.None
        <| db {
            getFstCol
                $"SELECT post_id FROM {db.tables.post} \
                  WHERE user_id = {mapped.Id} \
                  OR ({mapped.Permission} & {mask}) > (post_permission & {mask}) \
                  ORDER BY post_create_time DESC"
                []

            execute
        }

    member self.GetReadablePost() = self.GetPostGen(48uy)
    member self.GetWritablePost() = self.GetPostGen(12uy)
    member self.GetCommentablePost() = self.GetPostGen(3uy)

    member inline private self.GetCommentGen(mask: u8) =
        Seq.unfold
        <| fun list ->
            match list with
            | x :: xs ->
                let comment =
                    Comment(
                        palaflake,
                        mappedCommentProvider.fetch (coerce x),
                        mappedCommentProvider,
                        db,
                        mapped,
                        commentLogger
                    )
                    :> IComment

                Option.Some(comment, xs)
            | _ -> Option.None
        <| db {
            getFstCol
                $"SELECT comment_id FROM {db.tables.comment} \
                  WHERE user_id = {mapped.Id} \
                  OR ({mapped.Permission} & {mask}) > (comment_permission & {mask}) \
                  ORDER BY comment_create_time DESC"
                []

            execute
        }

    member self.GetReadableComment() = self.GetCommentGen(48uy)
    member self.GetWritableComment() = self.GetCommentGen(12uy)
    member self.GetCommentableComment() = self.GetCommentGen(3uy)

    member self.UpdateName newName =
        if self.WriteUserPermissionLv <> 0us then
            mapped.Name <- newName
            Ok()
        else
            $"Operation {nameof self.UpdateName} Failed: Permission denied"
            |> userLogger.error
            |> Err

    member self.UpdateEmail newEmail =
        if self.WriteUserPermissionLv <> 0us then
            mapped.Email <- newEmail
            Ok()
        else
            $"Operation {nameof self.UpdateEmail} Failed: Permission denied"
            |> userLogger.error
            |> Err

    member self.UpdatePermission newPermission =
        let handler_wu_lv = handler.Permission &&& 192us >>> 6
        let target_ru_lv = newPermission &&& 768us >>> 8
        let target_wu_lv = newPermission &&& 192us >>> 6
        let target_r_lv = newPermission &&& 48us >>> 4
        let target_w_lv = newPermission &&& 12us >>> 2
        let target_c_lv = newPermission &&& 3us

        //小于授权上级的用户的权限级别（防止管理员自克隆）
        if
            target_ru_lv >= handler_wu_lv
            || target_wu_lv >= handler_wu_lv
            || target_r_lv >= handler_wu_lv
            || target_w_lv >= handler_wu_lv
            || target_c_lv >= handler_wu_lv
        then
            $"Operation {nameof self.UpdatePermission} Failed: illegal permission({newPermission}) \
              (any target permission({newPermission}) must be lower than handler({self.Name})'s write user permission)"
            |> userLogger.error
            |> Err
        //保证可见性>=可评性>=可写性
        elif target_r_lv < target_c_lv then
            $"Operation {nameof self.UpdatePermission} Failed: illegal permission({newPermission}) \
              (violate constraint: read level >= comment level)"
            |> userLogger.error
            |> Err
        elif target_c_lv < target_w_lv then
            $"Operation {nameof self.UpdatePermission} Failed: illegal permission({newPermission}) \
              (violate constraint: comment level >= write level)"
            |> userLogger.error
            |> Err
        else
            mapped.Permission <- newPermission
            Ok()

    member self.UpdateItem itemName newValue =
        if self.WriteUserPermissionLv <> 0us then
            mapped.[itemName] <- Some newValue
            Ok()
        else
            $"Operation {nameof self.UpdateItem} Failed: Permission denied"
            |> userLogger.error
            |> Err

    member self.Drop() =
        //TODO handle UAF problem
        if self.WriteUserPermissionLv >= 2us then //TODO，暂不作实现，仅限pl_register(wu级别2)及root(wu级别3)访问
            mappedUserProvider.delete mapped.Id |> Ok
        else
            postLogger.error $"Operation {nameof self.Drop} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    interface IUser with
        member i.ReadPermissionLv = impl.ReadPermissionLv
        member i.WritePermissionLv = impl.WritePermissionLv
        member i.CommentPermissionLv = impl.CommentPermissionLv
        member i.ReadUserPermissionLv = impl.ReadUserPermissionLv
        member i.WriteUserPermissionLv = impl.WriteUserPermissionLv

        member i.Id = impl.Id
        member i.Name = impl.Name
        member i.Email = impl.Email
        member i.CreateTime = impl.CreateTime
        member i.AccessTime = impl.AccessTime
        member i.Permission = impl.Permission

        member i.Item x = impl.Item x

        member i.GetPost x = impl.GetPost x
        member i.GetComment x = impl.GetComment x
        member i.GetUser x = impl.GetUser x
        member i.UpdateItem x y = impl.UpdateItem x y

        member i.UpdateName x = impl.UpdateName x
        member i.UpdateEmail x = impl.UpdateEmail x
        member i.UpdatePermission x = impl.UpdatePermission x

        member i.GetReadablePost() = impl.GetReadablePost()
        member i.GetWritablePost() = impl.GetWritablePost()
        member i.GetCommentablePost() = impl.GetCommentablePost()
        member i.GetReadableComment() = impl.GetReadableComment()
        member i.GetWritableComment() = impl.GetWritableComment()
        member i.GetCommentableComment() = impl.GetCommentableComment()

        member i.NewUser x y z = impl.NewUser x y z
        member i.NewPost x y = impl.NewPost x y
        member i.Drop() = impl.Drop()
