namespace pilipala.pipeline.user

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.access.user
open pilipala.data.db
open pilipala.pipeline
open pilipala.access.user
open pilipala.pipeline.user

module IUserRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64, u64 * obj>>()

        { new IUserRenderPipelineBuilder with
            member i.Name = gen ()
            member i.Email = gen ()
            member i.CreateTime = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

type UserRenderPipeline internal (renderBuilder: IUserRenderPipelineBuilder, db: IDbOperationBuilder, user: IUser) =
    let get target (idVal: u64) =
        db {
            inUser
            getFstVal target "user_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let udf = Dict<string, u64 -> u64 * obj>()

    do
        for KV (name, builderItem) in renderBuilder do
            //udf管道初始为只会panic的GenericPipe，必须Replace后使用
            udf.Add(name, builderItem.fullyBuild id)

    member self.Name =
        renderBuilder.Name.fullyBuild
        <| fun fail id -> unwrapOr (get "user_name" id) (fun _ -> fail id)

    member self.Email =
        renderBuilder.Email.fullyBuild
        <| fun fail id -> unwrapOr (get "user_email" id) (fun _ -> fail id)

    member self.CreateTime =
        renderBuilder.CreateTime.fullyBuild
        <| fun fail id -> unwrapOr (get "user_create_time" id) (fun _ -> fail id)

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
