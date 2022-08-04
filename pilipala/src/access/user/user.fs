namespace pilipala.access.user

type User(mapped:IMappedUser)=
    member i.Id = mapped.Id

    member i.Name
        with get () = mapped.Name
        and set v = mapped.Name <- v

    member i.Email
        with get () = mapped.Email
        and set v = mapped.Email <- v

    member i.CreateTime
        with get () = mapped.CreateTime
        and set v = mapped.CreateTime <- v

    member i.AccessTime
        with get () = mapped.AccessTime
        and set v = mapped.AccessTime <- v

    member i.Permission
        with get () = mapped.Permission
        and set v = mapped.Permission <- v

    member i.Item
        with get name = mapped.[name]
        and set name v = mapped.[name] <- v

