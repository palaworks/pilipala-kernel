namespace pilipala.pipeline.user

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.user
open pilipala.access.user

module IUserModifyPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64 * obj>>()

        { new IUserModifyPipelineBuilder with
            member i.Name = gen ()
            member i.Email = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.Permission = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

type UserModifyPipeline internal (modifyBuilder: IUserModifyPipelineBuilder, db: IDbOperationBuilder) =
    let set targetKey (idVal: u64, targetVal) =
        match
            db {
                inUser
                update targetKey targetVal "user_id" idVal
                whenEq 1
                execute
            }
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let udf =
        Dict<string, u64 * obj -> u64 * obj>()

    do
        for KV (name, builderItem) in modifyBuilder do
            udf.Add(name, builderItem.fullyBuild id)

    member self.Name =
        modifyBuilder.Name.fullyBuild
        <| fun fail x -> unwrapOr (set "user_name" x) (fun _ -> fail x)

    member self.Email =
        modifyBuilder.Email.fullyBuild
        <| fun fail x -> unwrapOr (set "user_email" x) (fun _ -> fail x)

    member self.CreateTime =
        modifyBuilder.CreateTime.fullyBuild
        <| fun fail x -> unwrapOr (set "user_create_time" x) (fun _ -> fail x)

    member self.AccessTime =
        modifyBuilder.AccessTime.fullyBuild
        <| fun fail x -> unwrapOr (set "user_access_time" x) (fun _ -> fail x)

    member self.Permission =
        modifyBuilder.Permission.fullyBuild
        <| fun fail x -> unwrapOr (set "user_permission" x) (fun _ -> fail x)

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
