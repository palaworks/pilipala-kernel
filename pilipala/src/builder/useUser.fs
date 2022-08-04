[<AutoOpen>]
module pilipala.builder.useUser

open fsharper.typ
open Microsoft.Extensions.DependencyInjection
open pilipala.builder
open pilipala.data.db
open pilipala.access.user
(*
type Builder with

    member self.useUser(ld: LoginData) =

        let f (sc: IServiceCollection) = sc.AddSingleton<LoginData>(fun _ -> ld)

        { pipeline = self.pipeline .> f }
*)