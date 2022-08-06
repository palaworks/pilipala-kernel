namespace pilipala

open fsharper.op
open fsharper.typ
open fsharper.alias
open pilipala.id
open pilipala.data.db
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
        db: IDbOperationBuilder
    ) =

    member self.UserLogin(id: u64, pwd: string) =
        let sql =
            $"SELECT user_pwd_hash FROM {db.tables.user} WHERE user_id = <user_id>"
            |> db.managed.normalizeSql

        if db {
            select sql [ ("user_id", id) ]
            execute
           }
           |> bind
           <| fun x ->
               if pwd.bcrypt.Verify(coerce x) then
                   Some()
               else
                   None
           |> eq None then
            Err $"Invalid user id({id}) or password({pwd})"
        else
            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUserProvider.fetch id,
                db
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
        | None -> Err $"Invalid user name({name}) or password({pwd})"
        | Some id ->
            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUserProvider.fetch id,
                db
            )
            |> Ok
