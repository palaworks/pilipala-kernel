namespace pilipala.pipeline.post

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module IPostRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64, u64 * obj>>()

        { new IPostRenderPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen ()
            member i.UserId = gen ()
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

module IPostRenderPipeline =
    let make (renderBuilder: IPostRenderPipelineBuilder, db: IDbOperationBuilder) =
        let get target (idVal: u64) =
            db {
                inPost
                getFstVal target "post_id" idVal
                execute
            }
            |> fmap (fun v -> idVal, coerce v)

        let udf = Dict<string, u64 -> u64 * obj>()

        do
            for KV (name, builderItem) in renderBuilder do
                //udf管道初始为只会panic的GenericPipe，必须Replace后使用
                udf.Add(name, builderItem.fullyBuild id)

        let inline gen (builder: BuilderItem<_, _>) field a =
            builder.fullyBuild
            <| fun fail id -> unwrapOr (get field id) (fun _ -> fail id)
            |> apply a

        { new IPostRenderPipeline with
            member i.Title a = gen renderBuilder.Title "post_title" a

            member i.Body a = gen renderBuilder.Body "post_body" a

            member i.CreateTime a =
                gen renderBuilder.CreateTime "post_create_time" a

            member i.AccessTime a =
                gen renderBuilder.AccessTime "post_access_time" a

            member i.ModifyTime a =
                gen renderBuilder.ModifyTime "post_modify_time" a

            member i.UserId a = gen renderBuilder.UserId "user_id" a

            member i.Permission a =
                gen renderBuilder.Permission "post_permission" a

            member i.Item(name: string) = udf.TryGetValue(name).intoOption' () }
