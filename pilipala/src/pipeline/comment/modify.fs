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
open pilipala.data.db
open pilipala.pipeline

module CommentModifyPipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentModifyPipelineBuilder with
            member i.Body = gen () }
(*
            member self.nick = gen ()
            member self.content = gen ()
            member self.email = gen ()
            member self.site = gen ()
            member self.ctime = gen ()
            *)

type CommentModifyPipeline internal (modifyBuilder: ICommentModifyPipelineBuilder, dp: IDbProvider) =
    let set targetKey (idVal: u64, targetVal) =
        match dp
                  .mkCmd()
                  .update (dp.tables.comment, (targetKey, targetVal), ("commentId", idVal))
              <| eq 1
              |> dp.managed.executeQuery
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

    member self.Body =
        gen modifyBuilder.Body "comment_body"
(*
    member self.nick = gen modifyBuilder.nick "nick"

    member self.content =
        gen modifyBuilder.content "content"

    member self.email = gen modifyBuilder.email "email"

    member self.site = gen modifyBuilder.site "site"

    member self.ctime = gen modifyBuilder.ctime "ctime"
*)
