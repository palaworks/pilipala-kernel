namespace pilipala.pipeline.comment

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Pattern
open fsharper.op.Foldable
open pilipala.data.db
open pilipala.pipeline

module ICommentRenderPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<IGenericPipe<'I, 'I>>() }

        let udf = //user defined field
            Dict<string, BuilderItem<u64, u64 * obj>>()

        //site 交由插件实现
        //user_name交由用户组件实现（user_email）
        //email交由用户组件实现（user_email）
        //floor由comment_create_time推断
        //replies由comment_is_reply布尔决定：为true时视bind_id为回复到的comment_id
        { new ICommentRenderPipelineBuilder with
            member i.Body = gen ()
            member i.CreateTime = gen ()

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    let x = gen ()
                    udf.Add(name, x)
                    x

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()

            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

type CommentRenderPipeline internal (renderBuilder: ICommentRenderPipelineBuilder, db: IDbOperationBuilder) =
    let get targetKey (idVal: u64) =
        db {
            inComment
            getFstVal targetKey "comment_id" idVal
            execute
        }
        |> fmap (fun v -> idVal, coerce v)

    let udf =
        Dict<string, IGenericPipe<u64, u64 * obj>>()

    do
        for KV (name, builderItem) in renderBuilder do
            //udf管道初始为只会panic的GenericPipe，必须Replace后使用
            let justFail fail : IGenericPipe<_, _> = GenericPipe fail
            udf.Add(name, builderItem.fullyBuild justFail)

    member self.Body =
        renderBuilder.Body.fullyBuild
        <| fun fail -> GenericCachePipe(get "comment_body", fail)

    member self.CreateTime =
        renderBuilder.CreateTime.fullyBuild
        <| fun fail -> GenericCachePipe(get "comment_create_time", fail)

    member self.Item(name: string) = udf.TryGetValue(name).intoOption' ()
