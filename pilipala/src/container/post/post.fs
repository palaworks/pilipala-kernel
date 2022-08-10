namespace pilipala.container.post

open System
open Microsoft.Extensions.Logging
open dbm_test.PgSql
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.util.log
open pilipala.access.user
open pilipala.container.comment

type Post
    internal
    (
        palaflake: IPalaflakeGenerator,
        mapped: IMappedPost,
        mappedCommentProvider: IMappedCommentProvider,
        user: IMappedUser,
        postLogger: ILogger<Post>,
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

    member self.Title =
        if self.CanRead then
            Ok(mapped.Title)
        else
            postLogger.error $"Get {nameof self.Title} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.Body =
        if self.CanRead then
            Ok(mapped.Body)
        else
            postLogger.error $"Get {nameof self.Body} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.CreateTime =
        if self.CanRead then
            Ok(mapped.CreateTime)
        else
            postLogger.error $"Get {nameof self.CreateTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.AccessTime =
        if self.CanRead then
            Ok(mapped.AccessTime)
        else
            postLogger.error $"Get {nameof self.AccessTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.ModifyTime =
        if self.CanRead then
            Ok(mapped.ModifyTime)
        else
            postLogger.error $"Get {nameof self.ModifyTime} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.UserId =
        if self.CanRead then
            Ok(mapped.UserId)
        else
            postLogger.error $"Get {nameof self.UserId} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.Permission =
        if self.CanRead then
            Ok(mapped.Permission)
        else
            postLogger.error $"Get {nameof self.Permission} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.Item
        with get name =
            if self.CanRead then
                Ok(mapped.[name])
            else
                postLogger.error $"Get Item.{name} Failed: Permission denied (post id: {mapped.Id})"
                |> Err

    member self.UpdateTitle newTitle =
        if self.CanWrite then
            Ok(mapped.Title <- newTitle)
        else
            postLogger.error $"Operation {nameof self.UpdateTitle} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.UpdateBody newBody =
        if self.CanWrite then
            Ok(mapped.Body <- newBody)
        else
            postLogger.error $"Operation {nameof self.UpdateBody} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.UpdateItem name v =
        if self.CanRead then
            Ok(mapped.[name] <- v)
        else
            postLogger.error
                $"Operation {nameof self.UpdateItem}.{name} Failed: Permission denied (post id: {mapped.Id})"
            |> Err

    member self.NewComment(body: string) =
        if self.CanComment then
            { Id = palaflake.next ()
              Body = body
              CreateTime = DateTime.Now
              Binding = BindPost mapped.Id
              UserId = user.Id
              Permission =
                let r = (mapped.Permission &&& 48uy) //从文章继承的可见性

                r
                ||| u8 (user.Permission &&& 12us) //从用户继承的修改权
                ||| r //可评论性与可见性默认相同
              Item = always None }
            |> mappedCommentProvider.create
            |> fun x -> Comment(palaflake, x, mappedCommentProvider, user, commentLogger)
            |> Ok
        else
            postLogger.error $"Operation {nameof self.NewComment} Failed: Permission denied (post id: {mapped.Id})"
            |> Err
