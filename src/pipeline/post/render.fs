namespace pilipala.pipeline.post

open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline

module IPostRenderPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let title = gen ()
        let body = gen ()
        let createTime = gen ()
        let accessTime = gen ()
        let modifyTime = gen ()
        let userId = gen ()
        let permission = gen ()
        let udf = Dict<_, _>() //user defined field

        { new IPostRenderPipelineBuilder with
            member i.Title = title
            member i.Body = body
            member i.CreateTime = createTime
            member i.AccessTime = accessTime
            member i.ModifyTime = modifyTime
            member i.UserId = userId
            member i.Permission = permission

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    gen () |> effect (fun x -> udf.Add(name, x))

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()
            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

module IPostRenderPipeline =
    let make (renderBuilder: IPostRenderPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builderItem: BuilderItem<_, _>) field =
            let get target (idVal: i64) =
                db {
                    inPost
                    getFstVal target "post_id" idVal
                    execute
                }
                |> fmap (fun v -> idVal, coerce v)

            builderItem.fullyBuild
            <| fun fail id -> unwrapOrEval (get field id) (fun _ -> fail id)

        let title = gen renderBuilder.Title "post_title"
        let body = gen renderBuilder.Body "post_body"
        let createTime = gen renderBuilder.CreateTime "post_create_time"
        let accessTime = gen renderBuilder.AccessTime "post_access_time"
        let modifyTime = gen renderBuilder.ModifyTime "post_modify_time"
        let userId = gen renderBuilder.UserId "user_id"
        let permission = gen renderBuilder.Permission "post_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in renderBuilder do
                    dict.Add(name, builderItem.fullyBuild id)) //udf管道初始为只会panic的函数，必须Replace后使用

        { new IPostRenderPipeline with
            member i.Title a = title a
            member i.Body a = body a
            member i.CreateTime a = createTime a
            member i.AccessTime a = accessTime a
            member i.ModifyTime a = modifyTime a
            member i.UserId a = userId a
            member i.Permission a = permission a
            member i.Item(name: string) = udf.TryGetValue(name).intoOption' () }
