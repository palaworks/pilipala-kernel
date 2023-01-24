namespace pilipala.pipeline.comment

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline
open pilipala.container.comment

module ICommentModifyPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let body = gen ()
        let binding = gen ()
        let createTime = gen ()
        let modifyTime = gen ()
        let userId = gen ()
        let permission = gen ()
        let udf = Dict<_, _>()

        { new ICommentModifyPipelineBuilder with
            member i.Body = body
            member i.Binding = binding
            member i.CreateTime = createTime
            member i.ModifyTime = modifyTime
            member i.UserId = userId
            member i.Permission = permission

            member i.Item name =
                if udf.ContainsKey name then
                    udf.[name]
                else
                    gen () |> effect (fun x -> udf.Add(name, x))

            member i.GetEnumerator() : IEnumerator = udf.GetEnumerator()
            member i.GetEnumerator() : IEnumerator<_> = udf.GetEnumerator() }

module ICommentModifyPipeline =
    let make (modifyBuilder: ICommentModifyPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builder: BuilderItem<_>) field =
            let set targetKey (idVal: i64, targetVal) =
                match
                    db {
                        inComment
                        update targetKey targetVal "comment_id" idVal
                        whenEq 1
                        execute
                    }
                with
                | 1 -> Some(idVal, targetVal)
                | _ -> None

            builder.fullyBuild <| fun fail x -> unwrapOrEval (set field x) (fun _ -> fail x)

        let body =
            let set (idVal: i64, targetVal: string) =
                //TODO 该写法降低了对MySql数据库的兼容性(参数化标识符)，等待DbM补完或移除MySql支持性。
                let sql =
                    $"UPDATE {db.tables.comment} \
                      SET    comment_body        = :comment_body, \
                             comment_modify_time = :comment_modify_time \
                      WHERE  comment_id          = :comment_id"

                match
                    db {
                        query
                            sql
                            [ ("comment_body", (targetVal: obj))
                              ("comment_modify_time", (DateTime.Now: obj)) ]

                        whenEq 1
                        execute
                    }
                with
                | 1 -> Some(idVal, targetVal)
                | _ -> None

            modifyBuilder.Body.fullyBuild
            <| fun fail x -> unwrapOrEval (set x) (fun _ -> fail x)

        let binding =
            let setBinding v =
                let (comment_id: i64), comment_binding_id, comment_is_reply =
                    match v with
                    | x, BindPost y -> x, y, false
                    | x, BindComment y -> x, y, true

                if
                    db {
                        inComment
                        update "comment_binding_id" comment_binding_id "comment_id" comment_id
                        whenEq 1
                        execute
                    } = 1
                then
                    if
                        db {
                            inComment
                            update "comment_is_reply" comment_is_reply "comment_id" comment_id
                            whenEq 1
                            execute
                        } = 1
                    then
                        Some v
                    else
                        None
                else
                    None

            modifyBuilder.Binding.fullyBuild
            <| fun fail x -> unwrapOrEval (setBinding x) (fun _ -> fail x)

        let createTime = gen modifyBuilder.CreateTime "comment_create_time"
        let modifyTime = gen modifyBuilder.CreateTime "comment_modify_time"
        let userId = gen modifyBuilder.UserId "user_id"
        let permission = gen modifyBuilder.Permission "comment_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in modifyBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        { new ICommentModifyPipeline with
            member self.Body a = body a
            member self.Binding a = binding a
            member self.CreateTime a = createTime a
            member self.ModifyTime a = modifyTime a
            member self.UserId a = userId a
            member self.Permission a = permission a
            member self.Item(name: string) = udf.TryGetValue(name).intoOption' () }
