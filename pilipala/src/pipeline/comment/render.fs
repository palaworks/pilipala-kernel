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

type internal CommentRenderPipelineBuilder() =
    let gen () =
        { collection = List<PipelineCombineMode<'I, 'O>>()
          beforeFail = List<IGenericPipe<'I, 'I>>() }

    member self.nick: BuilderItem<u64, string> =
        gen ()

    member self.content: BuilderItem<u64, string> =
        gen ()

    member self.email: BuilderItem<u64, string> =
        gen ()

    member self.site: BuilderItem<u64, string> =
        gen ()

    member self.ctime: BuilderItem<u64, DateTime> =
        gen ()

type CommentRenderPipeline internal (builder: CommentRenderPipelineBuilder, dp: IDbProvider) =
    let get targetKey (idVal: u64) =
        dp
            .mkCmd()
            .getFstVal (dp.tables.comment, targetKey, "commentId", idVal)
        |> dp.managed.executeQuery
        >>= coerce

    let gen (builderItem: BuilderItem<_, _>) targetKey =
        let beforeFail =
            builderItem.beforeFail.foldr (fun p (acc: IGenericPipe<_, _>) -> acc.export p) (GenericPipe<_, _>(id))

        let data = get targetKey
        let fail = beforeFail.fill .> panicwith

        builderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.nick = gen builder.nick "cover"

    member self.content =
        gen builder.content "title"

    member self.email = gen builder.email "summary"

    member self.site = gen builder.site "body"

    member self.ctime = gen builder.ctime "ctime"
