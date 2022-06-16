module pilipala.palang.util

open System.Collections.Generic 
open fsharper.typ 

let isAlt (pattern: string) =
    pattern.StartsWith('<') && pattern.EndsWith('>')

let isOpt (pattern: string) =
    pattern.StartsWith('[') && pattern.EndsWith(']')

//para要么是alt（alternative）要么是opt（optional）
let isPara pattern = isAlt pattern || isOpt pattern
let paraName (pattern: string) = pattern.[1..^1]


let patternMatch (pattern: string) (command: string) =
    let pattern_list = pattern.Split(' ').toList ()
    let command_list = command.Split(' ').toList ()

    let z = zip pattern_list command_list //组合模式对

    let non_paras = filter (fun (p, _) -> not <| isPara p) z //过滤出所有非参数对

    if pattern_list.Length = command_list.Length //仅当模式长度=命令长度（大于情况出现在具有可选参数时）
       || (pattern_list.Length > command_list.Length
           && isOpt pattern_list.[^0]) //或模式长度>命令长度（出现在末尾具有可选参数时），
          && not <| any (fun (p, c) -> p <> c) non_paras then //且所有非参数对相等时，才存在匹配的必要

        let paras =
            filter (fun (p, _) -> isPara p) z //过滤出所有参数对
            |> map (fun (p, c) -> paraName p, c) //提取出参数名后重新组合

        let dic = Dictionary<string, string>()

        map dic.Add paras |> ignore

        Some dic
    else
        None