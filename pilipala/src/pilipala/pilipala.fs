namespace pilipala

open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment
open pilipala.data.db
open pilipala.id

type Pilipala
    internal
    (
        palaflake: IPalaflakeGenerator,
        mappedPostProvider: IMappedPostProvider,
        mappedCommentProvider: IMappedCommentProvider,
        mappedUserProvider: IMappedUserProvider,
        db: IDbOperationBuilder
    ) =


    member self.UserLogin id pwd =
        if db {
            inPost
            getFstVal "user_id" "user_id" id
            execute
        } = None then
            failwith "Invalid user id"
        else
            User(
                palaflake,
                mappedPostProvider,
                mappedCommentProvider,
                mappedUserProvider,
                mappedUserProvider.fetch id,
                db
            )
