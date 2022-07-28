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
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64, u64 * obj>>()

        //cover/summary/view/star 交由插件实现
        { new IPostRenderPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

type PostRenderPipeline internal (renderBuilder: IPostRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get target (idVal: u64) =
        db {
            inPost
            getFstVal target "post_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let udf =
        Dict<string, IGenericPipe<u64, u64 * obj>>()

    do
        for KV (name, builderItem) in renderBuilder do
            //udf管道初始为只会panic的GenericPipe，必须Replace后使用
            let justFail fail : IGenericPipe<_, _> = GenericPipe fail
            udf.Add(name, builderItem.fullyBuild justFail)

    member self.Title =
        renderBuilder.Title.fullyBuild
        <| fun fail -> GenericCachePipe(get "post_title", fail)

    member self.Body =
        renderBuilder.Body.fullyBuild
        <| fun fail -> GenericCachePipe(get "post_body", fail)

    member self.CreateTime =
        renderBuilder.CreateTime.fullyBuild
        <| fun fail -> GenericCachePipe(get "post_create_time", fail)

    member self.AccessTime =
        renderBuilder.AccessTime.fullyBuild
        <| fun fail -> GenericCachePipe(get "post_access_time", fail)

    member self.ModifyTime =
        renderBuilder.ModifyTime.fullyBuild
        <| fun fail -> GenericCachePipe(get "post_modify_time", fail)

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
