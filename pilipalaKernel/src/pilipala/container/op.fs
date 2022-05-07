[<AutoOpen>]
module pilipala.container.op

open fsharper.op.Alias

/// 将只读容器转为可变容器
let inline mut< ^c, ^mc when ^c: (member asMut : unit -> ^mc)> (c: ^c) = (^c: (member asMut : unit -> ^mc) c)

/// 使用容器
let inline using< ^c, ^r when ^c: (static member using : u64 -> ^r)> id =
    (^c: (static member using : u64 -> ^r) id)

/// 创建容器
let inline create< ^c, ^r when ^c: (static member create : unit -> ^r)> =
    (^c: (static member create : unit -> ^r) ())

/// 回收容器
let inline recycle< ^c, ^id, ^r when ^c: (static member recycle : ^id -> ^r)> id =
    (^c: (static member recycle : ^id -> ^r) id)

/// 抹除容器
let inline erase< ^c, ^id, ^r when ^c: (static member erase : ^id -> ^r)> id =
    (^c: (static member erase : ^id -> ^r) id)
