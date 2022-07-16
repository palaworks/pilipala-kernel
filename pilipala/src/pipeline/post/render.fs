namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.typ.Procedure
open fsharper.op
open fsharper.typ
open fsharper.op.Error
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Coerce
open fsharper.op.Foldable
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline

module IPostRenderPipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new IPostRenderPipelineBuilder with
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen () }
(*
            member self.cover = gen ()
            member self.title = gen ()
            member self.summary = gen ()
            member self.view = gen ()
            member self.star = gen ()
            *)

type PostRenderPipeline internal (renderBuilder: IPostRenderPipelineBuilder, dp: IDbProvider) =
    let get table target idKey (idVal: u64) =
        dp.mkCmd().getFstVal (table, target, idKey, idVal)
        |> dp.managed.executeQuery
        >>= coerce

    let gen (renderBuilderItem: BuilderItem<_, _>) table targetKey idKey =
        let beforeFail =
            renderBuilderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let data: u64 -> Option'<_> =
            get table targetKey idKey

        let fail: u64 -> _ =
            beforeFail.fill .> panicwith

        renderBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.Body =
        gen renderBuilder.Body dp.tables.record "post_body" "post_id"

    member self.CreateTime =
        gen renderBuilder.CreateTime dp.tables.meta "post_create_time" "post_id"

    member self.AccessTime =
        gen renderBuilder.AccessTime dp.tables.meta "post_access_time" "post_id"

    member self.ModifyTime =
        gen renderBuilder.ModifyTime dp.tables.record "post_modify_time" "post_id"

(*
    member self.title =
        gen renderBuilder.title dp.tables.record "title" "recordId"
    member self.cover =
        gen renderBuilder.cover dp.tables.record "cover" "recordId"
    member self.summary =
        gen renderBuilder.summary dp.tables.record "summary" "recordId"
    member self.view =
        gen renderBuilder.view dp.tables.meta "view" "metaId"
    member self.star =
        gen renderBuilder.star dp.tables.meta "star" "metaId"
*)
