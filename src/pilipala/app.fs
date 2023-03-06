namespace pilipala

open System
open Microsoft.Extensions.Logging
open fsharper.op
open fsharper.typ
open pilipala.id
open pilipala.data.db
open pilipala.util.log
open pilipala.util.hash
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment

type App
    internal
    (
        palaflake: IPalaflakeGenerator,
        mappedPostProvider: IMappedPostProvider,
        mappedCommentProvider: IMappedCommentProvider,
        mappedUserProvider: IMappedUserProvider,
        db: IDbOperationBuilder,
        mainLogger: ILogger<App>,
        postLogger: ILogger<Post>,
        commentLogger: ILogger<Comment>,
        userLogger: ILogger<User>
    ) as impl =

    member self.userLoginById id (pwd: string) =
        let sql =
            $"SELECT user_name, user_pwd_hash FROM {db.tables.user} WHERE user_id = :user_id"

        match
            db {
                getFstRow sql [ ("user_id", id) ]
                execute
            }
            |> bind
            <| fun x ->
                if { bcrypt = coerce x.["user_pwd_hash"] }.Verify pwd then
                    Some(id, coerce x.["user_name"])
                else
                    None
        with
        | None ->
            mainLogger.error $"User login failed: Invalid user id({id}) or password({pwd})"
            |> Exception
            |> Err
        | Some (id, name) ->
            mainLogger.info $"User login success: {name}" |> ignore

            let mappedUser = mappedUserProvider.fetch id
            mappedUser.AccessTime <- DateTime.Now //update access time

            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUser,
                mappedUser,
                db,
                postLogger.alwaysAppend $" (ops user: {name})",
                commentLogger.alwaysAppend $" (ops user: {name})",
                userLogger.alwaysAppend $" (ops user: {name})"
            )
            :> IUser
            |> Ok

    member self.userLoginByName name pwd =
        let sql =
            $"SELECT user_id, user_pwd_hash FROM {db.tables.user} WHERE user_name = :user_name"

        match
            db {
                getFstRow sql [ ("user_name", name) ]
                execute
            }
            |> bind
            <| fun x ->
                if { bcrypt = coerce x.["user_pwd_hash"] }.Verify pwd then
                    Some(coerce x.["user_id"])
                else
                    None
        with
        | None ->
            mainLogger.error $"User login failed: Invalid user name({name}) or password({pwd})"
            |> Exception
            |> Err
        | Some id ->
            mainLogger.info $"User login success: {name}" |> ignore

            let mappedUser = mappedUserProvider.fetch id
            mappedUser.AccessTime <- DateTime.Now //update access time

            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUser,
                mappedUser,
                db,
                postLogger.alwaysAppend $" (ops user: {name})",
                commentLogger.alwaysAppend $" (ops user: {name})",
                userLogger.alwaysAppend $" (ops user: {name})"
            )
            :> IUser
            |> Ok

    interface IApp with
        member i.userLoginById x y = impl.userLoginById x y
        member i.userLoginByName x y = impl.userLoginByName x y
