namespace pilipala.data

open System

[<AutoOpen>]
module op =

    let inline (<%>) m f = (^ma: (member fmap : ^f -> ^mb) m, f)

    let inline (<*>) ma mb =
        (^ma: (static member ap : ^ma -> ^mb -> ^mc) ma, mb)

    let inline (>>=) m f =
        (^ma: (member bind : (^v -> ^mb) -> ^mb) m, f)

    /// flatMap但不返回值
    let inline (>>=|) m f = m >>= f |> ignore

    let inline unwarp m = (^m: (member unwarp : unit -> ^v) m)

    let inline unwarpOr m value =
        (^m: (member unwarpOr : ^v -> ^v) m, value)

    let inline flatten m = m >>= id

    /// 对程序员友好的格式
    let inline debug m = (^m: (member debug : unit -> string) m)
    /// 打印对程序员友好对格式
    let inline debuglog m = debug m |> Console.WriteLine
