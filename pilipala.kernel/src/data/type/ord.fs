namespace pilipala.data

[<AutoOpen>]
module ord =

    type Ordering =
        | GT
        | EQ
        | LT
        

    type SortOrdering =
        | ASC
        | DESC

    
    let compare a b =
        if a > b then GT
        else if a = b then EQ
        else LT

    let inline eq a b = a = b