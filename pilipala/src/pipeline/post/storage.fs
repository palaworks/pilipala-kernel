namespace pilipala.pipeline.post

open System
open fsharper.op
open fsharper.op.Alias
open fsharper.typ
open fsharper.typ.Pipe
open DbManaged.PgSql
open pilipala.db
open pilipala.pipeline

module internal storage =
    let set table target idKey (idVal: u64, value) =
        mkCmd()
            .update(table, (target, value), (idKey, idVal))
            .whenEq(1)
            .executeQuery ()

    //可以在dataPipe添加逻辑，比如更新渲染管道缓存
    //这样，当数据库操作失败时，dataPipe逻辑将不被执行
    //出口可以追加与数据库访问成功与否无关的管道，例如日志记录
    let genMeta<'T> target =
        fun (idVal, value) ->
            let aff =
                set tables.meta target "metaId" (idVal, value)

            if aff = 1 then
                //如果数据库调用成功，None使得后备数据源（其他缓存逻辑）使用参数u64*'T执行
                None
            else
                //如果不成功，Some会让管道立即返回，终止其他逻辑的进行
                Some(idVal, value)
        |> CachePipe<u64 * 'T>
        |> ref

    let genRecord<'T> target =
        fun (idVal, value) ->
            let aff =
                set tables.record target "recordId" (idVal, value)

            if aff = 1 then
                None
            else
                Some(idVal, value)
        |> CachePipe<u64 * 'T>
        :> IPipe<_>
        |> ref

    let cover = genRecord<string> "cover"

    let title = genRecord<string> "title"

    let summary = genRecord<string> "summary"

    let body = genRecord<string> "body"

    let ctime = genMeta<DateTime> "ctime"

    let mtime = genRecord<DateTime> "mtime"

    let atime = genMeta<DateTime> "atime"

    let view = genMeta<u32> "view"

    let star = genMeta<u32> "star"

[<AutoOpen>]
module storage_typ =

    open storage

    type PostStoragePipeline internal () =
        let gen (p: Ref<CachePipe<'T>>) =
            { new IStoragePipeLine<'T> with
                member i.Before pipe =
                    p.Value <- CachePipe<'T>(pipe.fill .> p.Value.fill)
                    i

                member i.After pipe =
                    let mut = p.Value.asMut ()
                    mut.data <- mut.data .> pipe.fill
                    i

                member i.Replace f =
                    p.Value <- CachePipe<'T>((f p.Value).fill)
                    i }

        member self.cover = gen cover
        member self.title = gen title
        member self.summary = gen summary
        member self.body = gen body
        member self.ctime = gen ctime
        member self.mtime = gen mtime
        member self.atime = gen atime
        member self.view = gen view
        member self.star = gen star
