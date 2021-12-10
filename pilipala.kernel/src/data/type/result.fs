namespace pilipala.data

[<AutoOpen>]
module enhResult =
    open System

    type Result<'a, 'e> =
        | Ok of 'a
        | Err of 'e

        member inline self.fmap f =
            match self with
            | Ok x -> f x |> Ok
            | Err e -> Err e

        static member inline ap(ma: Result<'a -> 'b, 'e>, mb: Result<'a, 'e>) =
            match ma, mb with
            | Err e, _ -> Err e
            | Ok f, _ -> mb.fmap f

        member self.bind f =
            match self with
            | Err e -> Err e
            | Ok x -> f x

        member inline self.unwarp() =
            match self with
            | Ok x -> x
            | Err e -> e.ToString() |> Exception |> raise

        member inline self.unwarpOr f =
            match self with
            | Ok x -> x
            | _ -> f ()

        member inline self.debug() =
            match self with
            | Ok x ->
                //下一级调试信息
                let msg: string =
                    try
                        $"""({(x.tryInvoke "debug")})"""
                    with
                    | _ -> x.ToString()

                $"Ok {msg}"
            | Err e ->
                //下一级调试信息
                let msg: string =
                    try
                        $"""({(e.tryInvoke "debug")})"""
                    with
                    | _ -> e.ToString()

                $"Err {msg}"


(*
        member self.debug() =
            "["
            + (foldr
                (fun x acc ->
                    let str =
                        try
                            x
                                .GetType()
                                .GetMethod("debug")
                                .Invoke(x, [||])
                                .ToString()
                        with
                        | _ -> x.ToString()

                    $"; {str}{acc}")
                " ]"
                self.list)
                .Remove(0, 1)
        *)
