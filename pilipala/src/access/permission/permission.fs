namespace pilipala.access.permission

open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment
open pilipala.data.db

    (*
type PermissionProvider(user: IUser, db: IDbOperationBuilder) =

    member self.changeOwner(target: IPost, newOwner: IUser) =
        if user.Permission = 4095us then
            let aff =
                db {
                    inPost
                    update "user_id" newOwner.Id "post_id" target.Id
                    whenEq 1
                    execute
                } = 1

            if aff then
                Ok()
            else
                failwith $"Operation failed(affected:{aff})"
        else
            Err "Permission denied"

    member self.changeOwner(target: IComment, newOwner: IUser) =
        if user.Permission = 4095us then
            let aff =
                db {
                    inPost
                    update "user_id" newOwner.Id "post_id" target.Id
                    whenEq 1
                    execute
                } = 1

            if aff then
                Ok()
            else
                failwith $"Operation failed(affected:{aff})"
        else
            Err "Permission denied"
*)
    (*
    member self.changePermission(target: IUser, newPermission: u16) =
        if user.Permission = 4095us then
            let aff =
                db {
                    inUser
                    update "user_permission" newPermission "user_id" target.Id
                    whenEq 1
                    execute
                } = 1

            if aff then
                Ok()
            else
                failwith $"Operation failed(affected:{aff})"
        else
            Err "Permission denied"

    member self.changePermission(target: IPost, newPermission: u16) =
        if user.Permission = 4095us then
            let aff =
                db {
                    inPost
                    update "user_permission" newPermission "post_id" target.Id
                    whenEq 1
                    execute
                } = 1

            if aff then
                Ok()
            else
                failwith $"Operation failed(affected:{aff})"
        else
            Err "Permission denied"

    member self.changePermission(target: IComment, newPermission: u16) =
        if user.Permission = 4095us then
            let aff =
                db {
                    inComment
                    update "user_permission" newPermission "comment_id" target.Id
                    whenEq 1
                    execute
                } = 1

            if aff then
                Ok()
            else
                failwith $"Operation failed(affected:{aff})"
        else
            Err "Permission denied"
*)