namespace pilipala.data

open System
open FSharp.Interop.Dynamic

[<AutoOpen>]
module enhList =

    let rec private map f list =
        match list with
        | x :: xs -> (f x) :: map f xs
        | [] -> []

    let rec foldr f acc list =
        match list with
        | x :: xs -> f x (foldr f acc xs)
        | [] -> acc

    type List<'a>(init: 'a list) =

        new() = List []

        member private self.list: 'a list = init

        member self.fmap(f: 'a -> 'b) = map f self.list |> List

        static member ap(ma: List<'a -> 'b>, mb: List<'a>) =
            let rec ap lfs lxs =
                match lfs, lxs with
                | [], _ -> []
                | _, [] -> []
                | f :: fs, x :: xs -> (f x) :: ap fs xs

            ap ma.list mb.list |> List

        member self.bind(f: 'a -> 'b List) : 'b List =
            let f' x = (f x).list

            let rec foldr f acc list =
                match list with
                | x :: xs -> f x (foldr f acc xs)
                | [] -> acc

            let inline concat list = foldr (@) [] list

            map f' self.list |> concat |> List

        member self.unwarp() = self.list

        member self.debug() =
            "["
            + (foldr
                (fun x acc ->
                    //下一级调试信息
                    let msg: string =
                        try
                            x.tryInvoke "debug"
                        with
                        | _ -> x.ToString()

                    $"; {msg}{acc}")
                " ]"
                self.list)
                .Remove(0, 1)//去除首部分号
