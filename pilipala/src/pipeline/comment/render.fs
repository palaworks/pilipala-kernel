namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Error
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Foldable
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline

module ICommentRenderPipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentRenderPipelineBuilder with
            member i.Body = gen () }
(*
            member self.nick = gen ()
            member self.content = gen ()
            member self.email = gen ()
            member self.site = gen ()
            member self.ctime = gen ()
            *)

type CommentRenderPipeline internal (renderBuilder: ICommentRenderPipelineBuilder, dp: IDbProvider) =
    let get targetKey (idVal: u64) =
        dp
            .mkCmd()
            .getFstVal (dp.tables.comment, targetKey, "commentId", idVal)
        |> dp.managed.executeQuery
        >>= coerce

    let gen (renderBuilderItem: BuilderItem<_, _>) targetKey =
        let beforeFail =
            renderBuilderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let data = get targetKey
        let fail = beforeFail.fill .> panicwith

        renderBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.Body =
        gen renderBuilder.Body "cover"
(*
    member self.nick = gen renderBuilder.nick "cover"

    member self.content =
        gen renderBuilder.content "title"

    member self.email = gen renderBuilder.email "summary"

    member self.site = gen renderBuilder.site "body"

    member self.ctime = gen renderBuilder.ctime "ctime"
*)
