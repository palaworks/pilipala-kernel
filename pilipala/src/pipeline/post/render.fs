namespace pilipala.pipeline.post

open System
open System.Collections.Generic
open fsharper.op
open fsharper.typ
open fsharper.op.Alias
open fsharper.typ.Pipe
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline

type BuilderItem<'I, 'O> =
    { before: IGenericPipe<'I, 'I> List
      after: IGenericPipe<'O, 'O> List }

type internal PostRenderPipelineBuilder() =
    (*
    let get table target idKey (idVal: u64) =
        dp.mkCmd().getFstVal (table, target, idKey, idVal)
        |> dp.managed.executeQuery
        >>= coerce

    let coverNotFound = GenericPipe<u64, string>()

    //可以在原管道附加缓存层来建立缓存机制
    member self.genMeta<'T> target =
        let cache id = get dp.tables.meta target "metaId" id

        GenericCachePipe<u64, 'T>(cache = cache)

    member self.genRecord<'T> target =
        let cache id =
            get dp.tables.record target "recordId" id

        GenericCachePipe<u64, 'T>(cache = cache)
*)
    member self.cover =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<string, string>>() }

    member self.title =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<string, string>>() }

    member self.summary =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<string, string>>() }

    member self.body =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<string, string>>() }

    member self.ctime =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<DateTime, DateTime>>() }

    member self.mtime =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<DateTime, DateTime>>() }

    member self.atime =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<DateTime, DateTime>>() }

    member self.view =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<u32, u32>>() }

    member self.star =
        { before = List<IGenericPipe<u64, u64>>()
          after = List<IGenericPipe<u32, u32>>() }

type PostRenderPipeline internal (builder: PostRenderPipelineBuilder) =

    member self.cover =
        let before= foldr GenericPipe<u64,string>() builder.cover.before|>
    member self.title = gen builder.title
    member self.summary = gen builder.summary
    member self.body = gen builder.body
    member self.ctime = gen builder.ctime
    member self.mtime = gen builder.mtime
    member self.atime = gen builder.atime
    member self.view = gen builder.view
    member self.star = gen builder.star
