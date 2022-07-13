namespace pilipala.pipeline.comment

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

module CommentStoragePipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentStoragePipelineBuilder with
            member self.nick = gen ()
            member self.content = gen ()
            member self.email = gen ()
            member self.site = gen ()
            member self.ctime = gen () }

type CommentStoragePipeline internal (builder: ICommentStoragePipelineBuilder, dp: IDbProvider) =
    let set targetKey (idVal: u64, targetVal) =
        match dp
                  .mkCmd()
                  .update (dp.tables.comment, (targetKey, targetVal), ("commentId", idVal))
              <| eq 1
              |> dp.managed.executeQuery
            with
        | 1 -> Some(idVal, targetVal)
        | _ -> None

    let gen (builderItem: BuilderItem<_>) targetKey =
        let beforeFail =
            builderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let data = set targetKey
        let fail = beforeFail.fill .> panicwith

        builderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| CachePipe<_>(data, fail)

    member self.nick = gen builder.nick "nick"

    member self.content =
        gen builder.content "content"

    member self.email = gen builder.email "email"

    member self.site = gen builder.site "site"

    member self.ctime = gen builder.ctime "ctime"
