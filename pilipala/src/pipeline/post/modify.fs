namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.typ.Pipe
open fsharper.op.Alias
open fsharper.op.Error
open fsharper.op.Coerce
open fsharper.op.Foldable
open fsharper.typ.Procedure
open DbManaged.PgSql
open pilipala.data.db
open pilipala.pipeline

module IPostModifyPipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new IPostModifyPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen () }
(*
            member self.cover = gen ()//交由插件实现
            member self.summary = gen ()//交由插件实现
            member self.view = gen ()//交由插件实现
            member self.star = gen ()//交由插件实现
            *)

type PostModifyPipeline internal (modifyBuilder: IPostModifyPipelineBuilder, dp: IDbProvider) =
    let set table targetKey idKey (idVal: u64, targetVal) =
        match dp
                  .mkCmd()
                  .update (table, (targetKey, targetVal), (idKey, idVal))
              <| eq 1
              |> dp.managed.executeQuery
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (modifyBuilderItem: BuilderItem<_>) table targetKey idKey =
        let beforeFail =
            modifyBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let data = set table targetKey idKey

        let fail = beforeFail.fill .> panicwith

        modifyBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| CachePipe<_>(data, fail)

    member self.Body =
        gen modifyBuilder.Body dp.tables.record "body" "recordId"

    member self.CreateTime =
        gen modifyBuilder.CreateTime dp.tables.meta "ctime" "metaId"

    member self.AccessTime =
        gen modifyBuilder.AccessTime dp.tables.meta "atime" "metaId"

    member self.ModifyTime =
        gen modifyBuilder.ModifyTime dp.tables.record "mtime" "recordId"

(*
    member self.cover =
        gen modifyBuilder.cover dp.tables.record "cover" "recordId"

    member self.title =
        gen modifyBuilder.title dp.tables.record "title" "recordId"

    member self.summary =
        gen modifyBuilder.summary dp.tables.record "summary" "recordId"

    member self.view =
        gen modifyBuilder.view dp.tables.meta "view" "metaId"

    member self.star =
        gen modifyBuilder.star dp.tables.meta "star" "metaId"
*)
