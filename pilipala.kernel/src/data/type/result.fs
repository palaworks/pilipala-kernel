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

        member inline self.debug() = self.ToString()
