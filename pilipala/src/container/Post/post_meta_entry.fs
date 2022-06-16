namespace pilipala.container.Post

open System
open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open pilipala
open pilipala.container.cache
open pilipala.container
open DbManaged.PgSql.ext.String

type post_meta_entry internal (metaId: u64) =

    let cache =
        ContainerCacheHandler(db.tables.meta, "metaId", metaId)

    /// 元信息id
    member self.metaId = metaId


    /// 上级元信息id
    member self.baseMetaId
        with get (): u64 = cache.get "baseMetaId"
        and set (v: u64) = cache.set "baseMetaId" v
    /// 绑定记录id
    member self.bindRecordId
        with get (): u64 = cache.get "bindRecordId"
        and set (v: u64) = cache.set "bindRecordId" v
        
        
    /// 创建时间
    member self.ctime
        with get (): DateTime = cache.get "ctime"
        and set (v: DateTime) = cache.set "ctime" v
    /// 访问时间
    member self.atime
        with get (): DateTime = cache.get "atime"
        and set (v: DateTime) = cache.set "atime" v
        
        
    /// 访问数
    member self.view
        with get (): u32 = cache.get "view"
        and set (v: u32) = cache.set "view" v
    /// 星星数
    member self.star
        with get (): u32 = cache.get "star"
        and set (v: u32) = cache.set "star" v
