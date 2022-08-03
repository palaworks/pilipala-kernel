namespace pilipala.pipeline.user

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.op.Runtime
open fsharper.op.Foldable
open pilipala.id
open pilipala.data.db
open pilipala.pipeline
open pilipala.access.user

module IUserInitPipelineBuilder =
    let make () =
        let inline gen () =
            { collection = List<PipelineCombineMode<'I, 'O>>()
              beforeFail = List<'I -> 'I>() }

        { new IUserInitPipelineBuilder with
            member i.Batch = gen () }

type UserInitPipeline
    internal
    (
        initBuilder: IUserInitPipelineBuilder,
        palaflake: IPalaflakeGenerator,
        db: IDbOperationBuilder
    ) =
    let data (user: IUser, userPwdHash: string) =
        let user_id = palaflake.next ()

        let fields: (_ * obj) list =
            [ ("user_id", user_id)
              ("user_name", user.Name)
              ("user_email", user.Email)
              ("user_pwd_hash", userPwdHash)
              ("user_permission", user.Permission)
              ("user_create_time", user.CreateTime) ]

        let aff =
            db {
                inUser
                insert fields
                whenEq 1
                execute
            }

        if aff = 1 then Some user_id else None

    member self.Batch =
        initBuilder.Batch.fullyBuild
        <| fun fail x -> unwrapOr (data x) (fun _ -> fail x)
