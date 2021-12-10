namespace pilipala.data

open System

[<AutoOpen>]
module enhOption =

    /// 尝试拆箱None错误
    exception TryToUnwarpNone

    type Option<'a> =
        | Some of 'a
        | None

        member inline self.fmap f =
            match self with
            | Some a -> f a |> Some
            | None -> None

        static member inline ap(ma: Option<'a -> 'b>, mb: Option<'a>) =
            match ma, mb with
            | None, _ -> None
            | Some f, _ -> mb.fmap f

        member inline self.bind f =
            match self with
            | None -> None
            | Some x -> f x

        member inline self.unwarp() =
            match self with
            | Some x -> x
            | None -> raise TryToUnwarpNone

        member inline self.unwarpOr f =
            match self with
            | Some x -> x
            | None -> f ()

        member inline self.debug() =
            match self with
            | Some x ->
                //下一级调试信息
                let msg: string =
                    try
                        $"""({(x.tryInvoke "debug")})"""
                    with
                    | _ -> x.ToString()

                $"Some {msg}"
            | None -> "None"
