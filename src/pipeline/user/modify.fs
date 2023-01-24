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

module IUserModifyPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let name = gen ()
        let email = gen ()
        let createTime = gen ()
        let accessTime = gen ()
        let permission = gen ()
        let udf = Dict<_, _>()

        { new IUserModifyPipelineBuilder with
            member i.Name = name
            member i.Email = email
            member i.CreateTime = createTime
            member i.AccessTime = accessTime
            member i.Permission = permission

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    gen () |> effect (fun x -> udf.Add(name, x))

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()
            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

module IUserModifyPipeline =
    let make (modifyBuilder: IUserModifyPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builder: BuilderItem<_>) field =
            let set targetKey (idVal: i64, targetVal) =
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

            builder.fullyBuild <| fun fail x -> unwrapOrEval (set field x) (fun _ -> fail x)

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in modifyBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        let name = gen modifyBuilder.Name "user_name"
        let email = gen modifyBuilder.Email "user_email"
        let createTime = gen modifyBuilder.CreateTime "user_create_time"
        let accessTime = gen modifyBuilder.AccessTime "user_access_time"
        let permission = gen modifyBuilder.Permission "user_permission"

        { new IUserModifyPipeline with
            member self.Name a = name a
            member self.Email a = email a
            member self.CreateTime a = createTime a
            member self.AccessTime a = accessTime a
            member self.Permission a = permission a
            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
