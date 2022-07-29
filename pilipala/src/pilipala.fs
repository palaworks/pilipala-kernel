namespace pilipala

open fsharper.op.Alias
open pilipala.container.post
open pilipala.container.comment

type Pilipala internal (post: IPostProvider, comment: ICommentProvider) =
    member self.post = post
    member self.comment = comment
