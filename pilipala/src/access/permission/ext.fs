[<AutoOpen>]
module pilipala.access.permission.ext

open pilipala.access.user

type IUser with
    member self.Permission() = 1u
