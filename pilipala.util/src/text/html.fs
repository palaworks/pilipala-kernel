[<AutoOpen>]
module pilipala.util.text.html

open System
open System.Text.RegularExpressions

type Html = { html: string }

type Html with
    /// 去除html标签
    member self.withoutTags() =
        match self.html with
        | null
        | "" -> ""
        | _ ->
            let inline rm p i = Regex.Replace(i, p, "")

            self.html
            |> rm "<script[^>]*>(.|\n)*?</script>" //脚本标签
            |> rm "<style>(.|\n)*</style>" //样式标签
            |> rm "<([^>]|\n)+>" //其他标签
            |> rm "^\s*\n" //空行去除（不考虑最后一行，后续逻辑会将其去除）
            |> fun s -> Regex.Replace(s, "&#*\w+;", " ") //将转义替换为空格
            |> fun s -> Regex.Replace(s, "[\s\n]+", " ") //多空白合并
            |> rm "^ | $" //首尾去空格
