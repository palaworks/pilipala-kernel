namespace pilipala.container.post

open System
open Microsoft.Extensions.Logging
open fsharper.typ
open fsharper.alias
open pilipala.data.db
open pilipala.id
open pilipala.util.log
open pilipala.access.user
open pilipala.container.comment

type internal Post
    (
        palaflake: IPalaflakeGenerator,
        mapped: IMappedPost,
        mappedPostProvider: IMappedPostProvider,
        mappedCommentProvider: IMappedCommentProvider,
        db: IDbOperationBuilder,
        user: IMappedUser,
        postLogger: ILogger<Post>,
        commentLogger: ILogger<Comment>
    ) as impl =

    member _.CanRead =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 48us) > (mapped.Permission &&& 48uy)

    member _.CanWrite =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 12us) > (mapped.Permission &&& 12uy)

    member _.CanComment =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 3us) > (mapped.Permission &&& 3uy)

    member _.Id = mapped.Id

    member self.Title =
        if self.CanRead then
            Ok(mapped.Title)
        else
            postLogger.error $"Get {nameof self.Title} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.Body =
        if self.CanRead then
            Ok(mapped.Body)
        else
            postLogger.error $"Get {nameof self.Body} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.CreateTime =
        if self.CanRead then
            Ok(mapped.CreateTime)
        else
            postLogger.error $"Get {nameof self.CreateTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.AccessTime =
        if self.CanRead then
            Ok(mapped.AccessTime)
        else
            postLogger.error $"Get {nameof self.AccessTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.ModifyTime =
        if self.CanRead then
            Ok(mapped.ModifyTime)
        else
            postLogger.error $"Get {nameof self.ModifyTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.UserId =
        if self.CanRead then
            Ok(mapped.UserId)
        else
            postLogger.error $"Get {nameof self.UserId} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.Permission =
        if self.CanRead then
            Ok(mapped.Permission)
        else
            postLogger.error $"Get {nameof self.Permission} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.Item
        with get name =
            if self.CanRead then
                Ok(mapped.[name])
            else
                postLogger.error $"Get Item.{name} Failed: Permission denied (post id: {mapped.Id})"
                |> Exception
                |> Err

    member self.Comments =
        if self.CanRead then
            let sql =
                $"SELECT comment_id FROM {db.tables.comment} \
                  WHERE comment_is_reply = false AND comment_binding_id = {mapped.Id}"

            Seq.unfold
            <| fun (list: obj list) ->
                match list with
                | id :: ids ->
                    Option.Some(
                        Comment(
                            palaflake,
                            mappedCommentProvider.fetch (downcast id),
                            mappedCommentProvider,
                            db,
                            user,
                            commentLogger
                        )
                        :> IComment,
                        ids
                    )

                | [] -> Option.None
            <| db {
                getFstCol sql []
                execute
            }
            |> Ok
        else
            commentLogger.error $"Get {nameof self.Comments} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.UpdateTitle newTitle =
        if self.CanWrite then
            Ok(mapped.Title <- newTitle)
        else
            postLogger.error $"Operation {nameof self.UpdateTitle} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.UpdateBody newBody =
        if self.CanWrite then
            Ok(mapped.Body <- newBody)
        else
            postLogger.error $"Operation {nameof self.UpdateBody} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.UpdateItem name v =
        if self.CanWrite then
            Ok(mapped.[name] <- Some v)
        else
            postLogger.error
                $"Operation {nameof self.UpdateItem}.{name} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.UpdatePermission(permission: u8) =
        if self.CanWrite then
            if
                (permission &&& 48uy >>> 2) <= (permission &&& 12uy) //确保可写即可读
                && (permission &&& 48uy >>> 4) <= (permission &&& 3uy) //确保可评即可读
                && u8 (user.Permission &&& 48us) >= (permission &&& 48uy) //不允许过度提权
                && u8 (user.Permission &&& 12us) >= (permission &&& 12uy)
                && u8 (user.Permission &&& 3us) >= (permission &&& 3uy)
            then
                Ok(mapped.Permission <- permission)
            else
                postLogger.error
                    $"Operation {nameof self.UpdatePermission} Failed: Invalid permission updating (post id: {mapped.Id})"
                |> Exception
                |> Err
        else
            postLogger.error
                $"Operation {nameof self.UpdatePermission} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.NewComment(body: string) =
        if self.CanComment then
            let now = DateTime.Now

            { Id = palaflake.next ()
              Body = body
              CreateTime = now
              ModifyTime = now
              Binding = BindPost mapped.Id
              UserId = user.Id
              Permission =
                let r = (mapped.Permission &&& 48uy) //从文章继承的可见性

                r
                ||| u8 (user.Permission &&& 12us) //从用户继承的修改权
                ||| r //可评论性与可见性默认相同
              Props = Map [] }
            |> mappedCommentProvider.create
            |> fun x -> Comment(palaflake, x, mappedCommentProvider, db, user, commentLogger) :> IComment
            |> Ok
        else
            postLogger.error $"Operation {nameof self.NewComment} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    member self.Drop() =
        //TODO handle UAF problem
        if self.CanWrite then
            mappedPostProvider.delete mapped.Id |> Ok
        else
            postLogger.error $"Operation {nameof self.Drop} Failed: Permission denied (post id: {mapped.Id})"
            |> Exception
            |> Err

    interface IPost with

        member i.CanRead = impl.CanRead
        member i.CanWrite = impl.CanWrite
        member i.CanComment = impl.CanComment

        member i.Id = impl.Id
        member i.Title = impl.Title
        member i.Body = impl.Body
        member i.CreateTime = impl.CreateTime
        member i.AccessTime = impl.AccessTime
        member i.ModifyTime = impl.ModifyTime
        member i.UserId = impl.UserId
        member i.Permission = impl.Permission

        member i.Item x = impl.Item x
        member i.Comments = impl.Comments
        member i.UpdateTitle x = impl.UpdateTitle x
        member i.UpdateBody x = impl.UpdateBody x
        member i.UpdateItem x y = impl.UpdateItem x y
        member i.UpdatePermission x = impl.UpdatePermission x

        member i.NewComment x = impl.NewComment x
        member i.Drop() = impl.Drop()
