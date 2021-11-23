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
    /// 通常这类API使用递归+反射实现，会带来一定的性能损失。
    let inline debug m = (^m: (member debug : unit -> string) m)
    /// 打印对程序员友好对格式
    /// 通常这类API使用递归+反射实现，会带来一定的性能损失。
    let inline debugLog m = debug m |> Console.WriteLine
