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
open pilipala.db
open pilipala.pipeline

type internal PostStoragePipelineBuilder() =
    let gen () =
        { collection = List<PipelineCombineMode<'I, 'O>>()
          beforeFail = List<IGenericPipe<'I, 'I>>() }

    member self.cover: BuilderItem<u64 * string> =
        gen ()

    member self.title: BuilderItem<u64 * string> =
        gen ()

    member self.summary: BuilderItem<u64 * string> =
        gen ()

    member self.body: BuilderItem<u64 * string> =
        gen ()

    member self.ctime: BuilderItem<u64 * DateTime> =
        gen ()

    member self.mtime: BuilderItem<u64 * DateTime> =
        gen ()

    member self.atime: BuilderItem<u64 * DateTime> =
        gen ()

    member self.view: BuilderItem<u64 * u32> =
        gen ()

    member self.star: BuilderItem<u64 * u32> =
        gen ()

type PostStoragePipeline internal (builder: PostStoragePipelineBuilder, dp: IDbProvider) =
    let set table targetKey idKey (idVal: u64, targetVal) =
        match dp
                  .mkCmd()
                  .update (table, (targetKey, targetVal), (idKey, idVal))
              <| eq 1
              |> dp.managed.executeQuery
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (builderItem: BuilderItem<_>) table targetKey idKey =
        let beforeFail =
            builderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let data = set table targetKey idKey

        let fail = beforeFail.fill .> panicwith

        builderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| CachePipe<_>(data, fail)

    member self.cover =
        gen builder.cover dp.tables.record "cover" "recordId"

    member self.title =
        gen builder.title dp.tables.record "title" "recordId"

    member self.summary =
        gen builder.summary dp.tables.record "summary" "recordId"

    member self.body =
        gen builder.body dp.tables.record "body" "recordId"

    member self.ctime =
        gen builder.ctime dp.tables.meta "ctime" "metaId"

    member self.mtime =
        gen builder.mtime dp.tables.record "mtime" "recordId"

    member self.atime =
        gen builder.atime dp.tables.meta "atime" "metaId"

    member self.view =
        gen builder.view dp.tables.meta "view" "metaId"

    member self.star =
        gen builder.star dp.tables.meta "star" "metaId"
