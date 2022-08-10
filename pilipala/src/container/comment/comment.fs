namespace pilipala.container.comment

open System
open Microsoft.Extensions.Logging
open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.util.log
open pilipala.access.user

type Comment
    internal
    (
        palaflake: IPalaflakeGenerator,
        mapped: IMappedComment,
        mappedCommentProvider: IMappedCommentProvider,
        user: IMappedUser,
        commentLogger: ILogger<Comment>
    ) =

    member self.CanRead =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 48us) > (mapped.Permission &&& 48uy)

    member self.CanWrite =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 12us) > (mapped.Permission &&& 12uy)

    member self.CanComment =
        user.Id = mapped.UserId
        || u8 (user.Permission &&& 3us) > (mapped.Permission &&& 3uy)

    member self.Id = mapped.Id

    member self.Body =
        if self.CanRead then
            Ok(mapped.Body)
        else
            commentLogger.error $"Get {nameof self.Body} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.CreateTime =
        if self.CanRead then
            Ok(mapped.CreateTime)
        else
            commentLogger.error $"Get {nameof self.CreateTime} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.UserId =
        if self.CanRead then
            Ok(mapped.UserId)
        else
            commentLogger.error $"Get {nameof self.UserId} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.Permission =
        if self.CanRead then
            Ok(mapped.Permission)
        else
            commentLogger.error $"Get {nameof self.Permission} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.Item
        with get name =
            if self.CanRead then
                Ok(mapped.[name])
            else
                commentLogger.error $"Get Item.{name} Failed: Permission denied (comment id: {mapped.Id})"
                |> Err

    member self.UpdateBody newBody =
        if self.CanWrite then
            Ok(mapped.Body <- newBody)
        else
            commentLogger.error
                $"Operation {nameof self.UpdateBody} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.UpdateItem name v =
        if self.CanWrite then
            Ok(mapped.[name] <- v)
        else
            commentLogger.error
                $"Operation {nameof self.UpdateItem} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.UpdatePermission(permission: u8) =
        if self.CanWrite then
            if (permission &&& 48uy >>> 2)
               <= (permission &&& 12uy) //确保可写即可读
               && (permission &&& 48uy >>> 4)
                  <= (permission &&& 3uy) //确保可评即可读
               && u8 (user.Permission &&& 48us)
                  >= (permission &&& 48uy) //不允许过度提权
               && u8 (user.Permission &&& 12us)
                  >= (permission &&& 12uy)
               && u8 (user.Permission &&& 3us)
                  >= (permission &&& 3uy) then
                Ok(mapped.Permission <- permission)
            else
                commentLogger.error
                    $"Operation {nameof self.UpdatePermission} Failed: Invalid permission updating (comment id: {mapped.Id})"
                |> Err
        else
            commentLogger.error
                $"Operation {nameof self.UpdatePermission} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err

    member self.NewComment(body: string) =
        if self.CanComment then
            { Id = palaflake.next ()
              Body = body
              CreateTime = DateTime.Now
              Binding = BindComment mapped.Id
              UserId = user.Id
              Permission =
                let r = (mapped.Permission &&& 48uy) //从评论继承的可见性

                r
                ||| u8 (user.Permission &&& 12us) //从用户继承的修改权
                ||| r //可评论性与可见性默认相同
              Item = always None }
            |> mappedCommentProvider.create
            |> fun x -> Comment(palaflake, x, mappedCommentProvider, user, commentLogger)
            |> Ok
        else
            commentLogger.error
                $"Operation {nameof self.NewComment} Failed: Permission denied (comment id: {mapped.Id})"
            |> Err
