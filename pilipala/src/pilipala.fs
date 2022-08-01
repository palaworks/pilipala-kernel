namespace pilipala

open dbm_test.PgSql
open fsharper.op.Alias
open pilipala.access.user
open pilipala.container.post
open pilipala.container.comment

type Pilipala internal (post: IPostProvider, comment: ICommentProvider, user: IUserProvider) =

    member self.GetPost = post.fetch
    member self.NewPost = post.create

    member self.GetComment = comment.fetch
    member self.NewComment = comment.create

    member self.GetUser = user.fetch
    member self.NewUser = user.create
