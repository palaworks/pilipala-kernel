[<AutoOpen>]
module pilipala.container.comment.ext

type IComment with
    member i.UserName = ""
    member i.Replies: IComment list = []
