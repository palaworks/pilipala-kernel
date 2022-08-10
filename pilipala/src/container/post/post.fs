namespace pilipala.container.post

open System
open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.access.user
open pilipala.container.comment

type Post
    internal
    (
        palaflake: IPalaflakeGenerator,
        mapped: IMappedPost,
        mappedCommentProvider: IMappedCommentProvider,
        user: IMappedUser
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
            Err "Permission denied"

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

    member self.AccessTime =
        if self.CanRead then
            Ok(mapped.AccessTime)
        else
            Err "Permission denied"

    member self.ModifyTime =
        if self.CanRead then
            Ok(mapped.ModifyTime)
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

    member self.UpdateTitle newTitle =
        if self.CanWrite then
            Ok(mapped.Title <- newTitle)
        else
            Err "Permission denied"

    member self.UpdateBody newBody =
        if self.CanWrite then
            Ok(mapped.Body <- newBody)
        else
            Err "Permission denied"

    member self.UpdateItem name v =
        if self.CanRead then
            Ok(mapped.[name] <- v)
        else
            Err "Permission denied"

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
            |> fun x -> Comment(palaflake, x, mappedCommentProvider, user)
            |> Ok
        else
            Err "Permission denied"
