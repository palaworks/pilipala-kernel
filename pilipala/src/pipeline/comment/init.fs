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
open DbManaged
open pilipala.container.comment
open pilipala.db
open pilipala.id
open pilipala.pipeline

module CommentInitPipelineBuilder =
    let mk () =
        let gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        { new ICommentInitPipelineBuilder with
            member i.Batch = gen () }
(*
            member self.nick = gen ()
            member self.content = gen ()
            member self.email = gen ()
            member self.site = gen ()
            member self.ctime = gen ()
            *)

type CommentInitPipeline
    internal
    (
        initBuilder: ICommentInitPipelineBuilder,
        palaflake: IPalaflakeGenerator,
        db: IDbProvider
    ) =

    let data (comment: IComment) =

        let sql =
            $"INSERT INTO {db.tables.comment} \
              ( comment_id,  comment_body,  comment_create_time ) \
              VALUES \
              (<comment_id>,<comment_body>,<comment_create_time>)"
            |> db.managed.normalizeSql

        let comment_id = palaflake.next ()

        let paras: (_ * obj) list =
            [ ("comment_id", comment_id)
              ("comment_body", comment.Body)
              ("comment_create_time", DateTime.Now) ]

        let aff =
            db.mkCmd().query(sql, paras).whenEq 1
            |> db.managed.executeQuery

        if aff = 1 then
            Some(comment_id, comment)
        else
            None

    let gen (initBuilderItem: BuilderItem<_, _>) =
        let beforeFail =
            initBuilderItem.beforeFail.foldr (fun p (acc: IPipe<_>) -> acc.export p) (Pipe<_>())

        let fail = beforeFail.fill .> panicwith

        initBuilderItem.collection.foldl
        <| fun acc x ->
            match x with
            | Before before -> before.export acc
            | Replace f -> f acc
            | After after -> acc.export after
        <| GenericCachePipe<_, _>(data, fail)

    member self.Batch = gen initBuilder.Batch
(*
    member self.nick = gen initBuilder.nick "nick"

    member self.content =
        gen initBuilder.content "content"

    member self.email = gen initBuilder.email "email"

    member self.site = gen initBuilder.site "site"

    member self.ctime = gen initBuilder.ctime "ctime"
*)
