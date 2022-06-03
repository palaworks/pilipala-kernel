module pilipala.util.palaflake

open palaflake

let private g = Palaflaker(1uy, 2021us)
/// 生成palaflake
let gen () = g.Next()
