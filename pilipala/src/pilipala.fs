namespace pilipala

open fsharper.op.Alias
open pilipala.container.post
open pilipala.container.comment

type Pilipala internal (post: PostProvider, comment: CommentProvider) =
    member self.post = post
    member self.comment = comment
