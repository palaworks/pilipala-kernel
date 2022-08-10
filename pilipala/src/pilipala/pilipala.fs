namespace pilipala

open Microsoft.Extensions.Logging
open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.data.db
open pilipala.util.log
open pilipala.util.hash
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment

type Pilipala
    internal
    (
        palaflake: IPalaflakeGenerator,
        mappedPostProvider: IMappedPostProvider,
        mappedCommentProvider: IMappedCommentProvider,
        mappedUserProvider: IMappedUserProvider,
        db: IDbOperationBuilder,
        mainLogger: ILogger<Pilipala>,
        postLogger: ILogger<Post>,
        commentLogger: ILogger<Comment>,
        userLogger: Logger<User>
    ) =

    member self.UserLogin(id: u64, pwd: string) =
        let sql =
            $"SELECT user_name, user_pwd_hash FROM {db.tables.user} WHERE user_id = <user_id>"
            |> db.managed.normalizeSql

        match db {
                  getFstRow sql [ ("user_id", id) ]
                  execute
              }
              >>= fun x ->
                      if pwd.bcrypt.Verify(coerce x.["user_pwd_hash"]) then
                          Some(id, coerce x.["user_name"])
                      else
                          None
            with
        | None ->
            mainLogger.error $"User login failed: Invalid user id({id}) or password({pwd})"
            |> Err
        | Some (id, name) ->
            mainLogger.info $"User login success: {name}"
            |> ignore

            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUserProvider.fetch id,
                db,
                postLogger.alwaysAppend $" (ops user: {name})",
                commentLogger.alwaysAppend $" (ops user: {name})",
                userLogger.alwaysAppend $" (ops user: {name})"
            )
            |> Ok

    member self.UserLogin(name, pwd: string) =
        let sql =
            $"SELECT user_id, user_pwd_hash FROM {db.tables.user} WHERE user_name = <user_name>"
            |> db.managed.normalizeSql

        match db {
                  getFstRow sql [ ("user_name", name) ]
                  execute
              }
              >>= fun x ->
                      if pwd.bcrypt.Verify(coerce x.["user_pwd_hash"]) then
                          Some(coerce x.["user_id"])
                      else
                          None
            with
        | None ->
            mainLogger.error $"User login failed: Invalid user id({id}) or password({pwd})"
            |> Err
        | Some id ->
            mainLogger.info $"User login success: {name}"
            |> ignore

            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUserProvider.fetch id,
                db,
                postLogger.alwaysAppend $" (ops user: {name})",
                commentLogger.alwaysAppend $" (ops user: {name})",
                userLogger.alwaysAppend $" (ops user: {name})"
            )
            |> Ok
