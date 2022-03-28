[<AutoOpen>]
module pilipala.container.op


/// 创建容器
let inline create< ^c, ^r when ^c: (static member create : unit -> ^r)> =
    (^c: (static member create : unit -> ^r) ())

/// 回收容器
let inline recycle< ^c, ^id, ^r when ^c: (static member recycle : ^id -> ^r)> id =
    (^c: (static member recycle : ^id -> ^r) id)

/// 抹除容器
let inline erase< ^c, ^id, ^r when ^c: (static member erase : ^id -> ^r)> id =
    (^c: (static member erase : ^id -> ^r) id)

//TODO:简化API抽象