namespace pilipala.container.post

open System
open System.Collections.Generic
open fsharper.typ.Procedure
open fsharper.op
open fsharper.typ
open fsharper.op.Error
open fsharper.op.Alias
open fsharper.typ.Pipe
open fsharper.op.Coerce
open fsharper.op.Foldable
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline
open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open DbManaged
open DbManaged.PgSql
open pilipala
open pilipala.container.Post
open pilipala.container
open pilipala.db
open pilipala.id
open pilipala.pipeline.post

(*
type Post() =
     /// 标题
      Title: string
      /// 正文
      Body: string

      /// 创建时间
      CreateTime: DateTime
      /// 访问时间
      AccessTime: DateTime
      /// 修改时间
      ModifyTime: DateTime
*)


type PostRecordProvider(render: PostRenderPipeline, modify: PostModifyPipeline, init: PostInitPipeline) =

    member self.fetch(post_id: u64) =
        { new IPost with
            member i.Body
                with get () = snd (render.Body.fill post_id)
                and set v = modify.Body.fill (post_id, v) |> ignore

            member i.CreateTime
                with get () = snd (render.CreateTime.fill post_id)
                and set v = modify.CreateTime.fill (post_id, v) |> ignore

            member i.AccessTime
                with get () = snd (render.AccessTime.fill post_id)
                and set v = modify.AccessTime.fill (post_id, v) |> ignore

            member i.ModifyTime
                with get () = snd (render.ModifyTime.fill post_id)
                and set v = modify.ModifyTime.fill (post_id, v) |> ignore }

    member self.create(post: IPost) = fst (init.Batch.fill post)
(*
module IPostMetaEntry =
    let mk (metaId: u64) =
        let render = PostRenderPipeline()
        let storage = PostStoragePipeline()

        { new IPostMetaEntry with

            member self.metaId = metaId

            member self.baseMetaId: u64 = render.get "baseMetaId"

            member self.baseMetaId
                with set (v: u64) = cache.set "baseMetaId" v

            member self.bindRecordId: u64 = cache.get "bindRecordId"

            member self.bindRecordId
                with set (v: u64) = cache.set "bindRecordId" v

            member self.ctime: DateTime = cache.get "ctime"

            member self.ctime
                with set (v: DateTime) = cache.set "ctime" v

            member self.atime: DateTime = cache.get "atime"

            member self.atime
                with set (v: DateTime) = cache.set "atime" v

            member self.view: u32 = cache.get "view"

            member self.view
                with set (v: u32) = cache.set "view" v

            member self.star: u32 = cache.get "star"

            member self.star
                with set (v: u32) = cache.set "star" v }

module IPostRecordEntry =
    let mk (recordId: u64) =

        let cache = ContainerCacheHandler(db.tables.record, "recordId", recordId)

        { new IPostRecordEntry with

            member self.recordId = recordId

            member self.cover: string = cache.get "cover"

            member self.cove
                with set (v: string) = cache.set "cover" v

            member self.title: string = cache.get "title"

            member self.title
                with set (v: string) = cache.set "title" v

            member self.summary: string = cache.get "summary"

            member self.summary
                with set (v: string) = cache.set "summary" v

            member self.body: string = cache.get "body"

            member self.body
                with set (v: string) = cache.set "body" v

            member self.mtime: DateTime = cache.get "mtime"

            member self.mtime
                with set (v: DateTime) = cache.set "mtime" v }
*)
