namespace pilipala.container.comment

open System
open fsharper.typ
open pilipala.id
open pilipala.access.user

type Comment
    internal
    (
        palaflake: IPalaflakeGenerator,
        mapped: IMappedComment,
        mappedCommentProvider: IMappedCommentProvider,
        user: IMappedUser
    ) =

    member self.CanRead =
        user.Id = mapped.UserId
        || (user.Permission &&& 3072us) > (mapped.Permission &&& 3072us)

    member self.CanWrite =
        user.Id = mapped.UserId
        || (user.Permission &&& 300us) > (mapped.Permission &&& 300us)

    member self.CanReadComment =
        user.Id = mapped.UserId
        || (user.Permission &&& 192us) > (mapped.Permission &&& 192us)

    member self.CanWriteComment =
        user.Id = mapped.UserId
        || (user.Permission &&& 48us) > (mapped.Permission &&& 48us)

    member self.Id = mapped.Id

    member self.Body =
        if self.CanRead then
            Ok(mapped.Body)
        else
            Err "Permission denied"

    member self.CreateTime =
        if self.CanRead then
            Ok(mapped.CreateTime)
        else
            Err "Permission denied"

    member self.UserId =
        if self.CanRead then
            Ok(mapped.UserId)
        else
            Err "Permission denied"

    member self.Permission =
        if self.CanRead then
            Ok(mapped.Permission)
        else
            Err "Permission denied"

    member self.Item
        with get name =
            if self.CanRead then
                Ok(mapped.[name])
            else
                Err "Permission denied"

    member self.UpdateBody newBody =
        if self.CanWrite then
            Ok(mapped.Body <- newBody)
        else
            Err "Permission denied"

    member self.UpdateItem name v =
        if self.CanWrite then
            Ok(mapped.[name] <- v)
        else
            Err "Permission denied"

    member self.NewComment(body: string) =
        if self.CanWriteComment then
            { Id = palaflake.next ()
              Body = body
              CreateTime = DateTime.Now
              Binding = BindComment mapped.Id
              UserId = user.Id
              Permission = user.Permission
              Item = always None }
            |> mappedCommentProvider.create
            |> fun x -> Comment(palaflake, x, mappedCommentProvider, user)
            |> Ok
        else
            Err "Permission denied"
