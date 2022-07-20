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
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        //cover/summary/view/star 交由插件实现
        { new IPostModifyPipelineBuilder with
            member i.Title = gen ()
            member i.Body = gen ()
            member i.CreateTime = gen ()
            member i.AccessTime = gen ()
            member i.ModifyTime = gen () }

type PostModifyPipeline internal (modifyBuilder: IPostModifyPipelineBuilder, db: IDbProvider) =
    let set targetKey (idVal: u64, targetVal) =
        match db
                  .makeCmd()
                  .update (db.tables.post, (targetKey, targetVal), ("post_id", idVal))
              <| eq 1
              |> db.managed.executeQuery
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (modifyBuilderItem: BuilderItem<_>) targetKey =
        let beforeFail =
            modifyBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let data = set targetKey

        let fail = beforeFail.fill .> panicwith

        modifyBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| CachePipe<_>(data, fail)

    member self.Title =
        gen modifyBuilder.Title "body"

    member self.Body =
        gen modifyBuilder.Body "body"

    member self.CreateTime =
        gen modifyBuilder.CreateTime "ctime"

    member self.AccessTime =
        gen modifyBuilder.AccessTime "atime"

    member self.ModifyTime =
        gen modifyBuilder.ModifyTime "mtime"
