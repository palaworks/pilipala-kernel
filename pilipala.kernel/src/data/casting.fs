namespace pilipala.data

[<AutoOpen>]
module casting =

    open System

    let inline cast (a: 'a) : 'b =
        downcast Convert.ChangeType(a, typeof<'b>)
