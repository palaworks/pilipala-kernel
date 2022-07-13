namespace pilipala.container.Post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.container.Post
open pilipala.container

open pilipala.pipeline.post


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
