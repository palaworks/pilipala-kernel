[<AutoOpen>]
module pilipala.container.post.ext

open pilipala.container.comment

type IPost with
    member i.Comments: IComment list =
        
        []
        
