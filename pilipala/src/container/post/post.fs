namespace pilipala.container.post

open fsharper.op.Alias
open pilipala.access.user

type Post(post_id: u64, provider: IMappedPostProvider, user: IMappedUser) =

    let mapped = provider.fetch post_id
    member i.Id = post_id

    member i.Title
        with get () = mapped.Title
        and set v = mapped.Title <- v

    member i.Body
        with get () = mapped.Body
        and set v = mapped.Body <- v

    member i.CreateTime
        with get () = mapped.CreateTime
        and set v = mapped.CreateTime <- v

    member i.AccessTime
        with get () = mapped.AccessTime
        and set v = mapped.AccessTime <- v

    member i.ModifyTime
        with get () = mapped.ModifyTime
        and set v = mapped.ModifyTime <- v

    member i.UserId
        with get () = mapped.UserId
        and set v = mapped.UserId <- v

    member i.Permission
        with get () = mapped.Permission
        and set v = mapped.Permission <- v

    member i.Item
        with get name = mapped.[name]
        and set name v = mapped.[name] <- v
