namespace pilipala.pipeline.post

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
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
        for kv in renderBuilder do
            udf.Add(kv.Key, fullyBuild (always None) kv.Value)

    member self.Title =
        fullyBuild (get "post_title") renderBuilder.Title

    member self.Body =
        fullyBuild (get "post_body") renderBuilder.Body

    member self.CreateTime =
        fullyBuild (get "post_create_time") renderBuilder.CreateTime

    member self.AccessTime =
        fullyBuild (get "post_access_time") renderBuilder.AccessTime

    member self.ModifyTime =
        fullyBuild (get "post_modify_time") renderBuilder.ModifyTime

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
