namespace pilipala.pipeline.post

open System
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

        //cover/summary/view/star 交由插件实现
        { new IPostRenderPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen () }

type PostRenderPipeline internal (renderBuilder: IPostRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get target (idVal: u64) =
        db {
            inPost
            getFstVal target "post_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let gen (renderBuilderItem: BuilderItem<_, _>) targetKey =
        let beforeFail =
            renderBuilderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let data: u64 -> Option'<_> = get targetKey

        let fail: u64 -> _ =
            beforeFail.fill .> panicwith

        renderBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.Title =
        gen renderBuilder.Title "post_title"

    member self.Body =
        gen renderBuilder.Body "post_body"

    member self.CreateTime =
        gen renderBuilder.CreateTime "post_create_time"

    member self.AccessTime =
        gen renderBuilder.AccessTime "post_access_time"

    member self.ModifyTime =
        gen renderBuilder.ModifyTime "post_modify_time"
