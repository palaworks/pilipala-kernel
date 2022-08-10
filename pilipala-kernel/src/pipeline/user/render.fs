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
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64, u64 * obj>>()

        { new IUserRenderPipelineBuilder with
            member i.Name = gen ()
            member i.Email = gen ()
            member i.Permission = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()

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

        let inline gen (builder: BuilderItem<_, _>) field a =
            builder.fullyBuild
            <| fun fail id -> unwrapOr (get field id) (fun _ -> fail id)
            |> apply a

        { new IUserRenderPipeline with
            member self.Name a = gen renderBuilder.Name "user_name" a

            member self.Email a = gen renderBuilder.Email "user_email" a

            member self.CreateTime a =
                gen renderBuilder.CreateTime "user_create_time" a

            member self.AccessTime a =
                gen renderBuilder.AccessTime "user_access_time" a

            member self.Permission a =
                gen renderBuilder.Permission "user_permission" a

            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
