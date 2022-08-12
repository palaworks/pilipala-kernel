namespace pilipala.pipeline.user

open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline
open pilipala.pipeline.user

module IUserRenderPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let name = gen ()
        let email = gen ()
        let permission = gen ()
        let createTime = gen ()
        let accessTime = gen ()
        let udf = Dict<_, _>()

        { new IUserRenderPipelineBuilder with
            member i.Name = name
            member i.Email = email
            member i.Permission = permission
            member i.CreateTime = createTime
            member i.AccessTime = accessTime

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()
            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

module IUserRenderPipeline =
    let make (renderBuilder: IUserRenderPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builder: BuilderItem<_, _>) field =
            let get target (idVal: i64) =
                db {
                    inUser
                    getFstVal target "user_id" idVal
                    execute
                }
                |> fmap (fun v -> idVal, coerce v)

            builder.fullyBuild
            <| fun fail id -> unwrapOr (get field id) (fun _ -> fail id)

        let name =
            gen renderBuilder.Name "user_name"

        let email =
            gen renderBuilder.Email "user_email"

        let createTime =
            gen renderBuilder.CreateTime "user_create_time"

        let accessTime =
            gen renderBuilder.AccessTime "user_access_time"

        let permission =
            gen renderBuilder.Permission "user_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in renderBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        { new IUserRenderPipeline with
            member self.Name a = name a
            member self.Email a = email a
            member self.CreateTime a = createTime a
            member self.AccessTime a = accessTime a
            member self.Permission a = permission a
            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
