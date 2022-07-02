namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.typ.Procedure
open fsharper.op
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Coerce
open fsharper.op.Foldable
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline

type BuilderItem<'I, 'O> =
    { before: IGenericPipe<'I, 'I> List
      after: IGenericPipe<'O, 'O> List
      beforeFail: IGenericPipe<'I, 'I> List }

type internal PostRenderPipelineBuilder() =
    let gen () =
        { before = List<IGenericPipe<'I, 'I>>()
          after = List<IGenericPipe<'O, 'O>>()
          beforeFail = List<IGenericPipe<'I, 'I>>() }

    member self.cover: BuilderItem<u64, string> = gen ()
    member self.title: BuilderItem<u64, string> = gen ()
    member self.summary: BuilderItem<u64, string> = gen ()
    member self.body: BuilderItem<u64, string> = gen ()

    member self.ctime: BuilderItem<u64, DateTime> = gen ()
    member self.mtime: BuilderItem<u64, DateTime> = gen ()
    member self.atime: BuilderItem<u64, DateTime> = gen ()

    member self.view: BuilderItem<u64, u32> = gen ()
    member self.star: BuilderItem<u64, u32> = gen ()

type PostRenderPipeline internal (builder: PostRenderPipelineBuilder, dp: DbProvider) =
    let get table target idKey (idVal: u64) =
        dp.mkCmd().getFstVal (table, target, idKey, idVal)
        |> dp.managed.executeQuery
        >>= coerce

    let gen (builderItem: BuilderItem<_, _>) table target idKey =
        let before =
            builderItem.before.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let after =
            builderItem.after.foldl (fun (acc: IGenericPipe<_, _>) -> acc.export) (GenericPipe<_, _>(id))

        let beforeFail =
            builderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let fail id =
            beforeFail.fill id |> fun id -> failwith $"{id}"

        let data id = get table target idKey id

        before
            .export(GenericCachePipe<_, _>(data, fail))
            .export after

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
