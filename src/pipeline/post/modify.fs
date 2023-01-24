namespace pilipala.pipeline.post

open System
open System.Collections
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.alias
open fsharper.op.Pattern
open pilipala.data.db
open pilipala.pipeline

module IPostModifyPipelineBuilder =
    let make () =

        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let title = gen ()
        let body = gen ()
        let createTime = gen ()
        let accessTime = gen ()
        let modifyTime = gen ()
        let userId = gen ()
        let permission = gen ()
        let udf = Dict<_, _>() //user defined field

        { new IPostModifyPipelineBuilder with
            member i.Title = title
            member i.Body = body
            member i.CreateTime = createTime
            member i.AccessTime = accessTime
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

module IPostModifyPipeline =
    let make (modifyBuilder: IPostModifyPipelineBuilder, db: IDbOperationBuilder) =

        let inline gen (builder: BuilderItem<_>) field =
            let set targetKey (idVal: i64, targetVal) =
                match
                    db {
                        inPost
                        update targetKey targetVal "post_id" idVal
                        whenEq 1
                        execute
                    }
                with
                | 1 -> Some(idVal, targetVal)
                | _ -> None

            builder.fullyBuild <| fun fail x -> unwrapOrEval (set field x) (fun _ -> fail x)

        let title = gen modifyBuilder.Title "post_title"

        let body =
            let set (idVal: i64, targetVal: string) =
                //TODO 该写法降低了对MySql数据库的兼容性(参数化标识符)，等待DbM补完或移除MySql支持性。
                let sql =
                    $"UPDATE {db.tables.post} \
                      SET    post_body        = :post_body, \
                             post_modify_time = :post_modify_time \
                      WHERE  post_id          = :post_id"

                match
                    db {
                        query sql [ ("post_body", (targetVal: obj)); ("post_modify_time", (DateTime.Now: obj)) ]
                        whenEq 1
                        execute
                    }
                with
                | 1 -> Some(idVal, targetVal)
                | _ -> None

            modifyBuilder.Body.fullyBuild
            <| fun fail x -> unwrapOrEval (set x) (fun _ -> fail x)

        let createTime = gen modifyBuilder.CreateTime "post_create_time"

        let accessTime = gen modifyBuilder.AccessTime "post_access_time"

        let modifyTime = gen modifyBuilder.ModifyTime "post_modify_time"

        let userId = gen modifyBuilder.UserId "user_id"

        let permission = gen modifyBuilder.Permission "post_permission"

        let udf =
            Dict<_, _>()
            |> effect (fun dict ->
                for KV (name, builderItem) in modifyBuilder do
                    dict.Add(name, builderItem.fullyBuild id))

        { new IPostModifyPipeline with
            member i.Title a = title a
            member i.Body a = body a
            member i.CreateTime a = createTime a
            member i.AccessTime a = accessTime a
            member i.ModifyTime a = modifyTime a
            member i.UserId a = userId a
            member i.Permission a = permission a
            member i.Item(name: string) = udf.TryGetValue(name).intoOption' () }
