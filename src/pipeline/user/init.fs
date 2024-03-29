namespace pilipala.pipeline.user

open System.Collections.Generic
open fsharper.op
open fsharper.typ
open pilipala.id
open pilipala.data.db
open pilipala.pipeline
open pilipala.util.hash
open pilipala.access.user

module IUserInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<_>()
              beforeFail = List<_>() }

        let batch = gen ()

        { new IUserInitPipelineBuilder with
            member i.Batch = batch }

module IUserInitPipeline =
    let make (initBuilder: IUserInitPipelineBuilder, uuid: IUuidGenerator, db: IDbOperationBuilder) =
        let data (user: UserData) =
            let fields: (_ * obj) list =
                [ ("user_id", user.Id)
                  ("user_name", user.Name)
                  ("user_email", user.Email)
                  ("user_pwd_hash", uuid.next().bcrypt) //默认随机密码
                  ("user_permission", user.Permission)
                  ("user_create_time", user.CreateTime) ]

            let aff =
                db {
                    inUser
                    insert fields
                    whenEq 1
                    execute
                }

            if aff = 1 then Some user.Id else None

        let batch =
            initBuilder.Batch.fullyBuild
            <| fun fail x -> unwrapOrEval (data x) (fun _ -> fail x)

        { new IUserInitPipeline with
            member self.Batch a = batch a }
