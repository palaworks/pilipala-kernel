namespace pilipala.pipeline.comment

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module ICommentRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        //site 交由插件实现
        //user_name交由用户组件实现（user_email）
        //email交由用户组件实现（user_email）
        //floor由comment_create_time推断
        //replies由comment_is_reply布尔决定：为true时视bind_id为回复到的comment_id
        { new ICommentRenderPipelineBuilder with
            member i.Body = gen ()
            member i.CreateTime = gen () }

type CommentRenderPipeline internal (renderBuilder: ICommentRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get targetKey (idVal: u64) =
        db {
            inComment
            getFstVal targetKey "comment_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

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
        gen renderBuilder.Body "comment_body"

    member self.CreateTime =
        gen renderBuilder.CreateTime "comment_create_time"
