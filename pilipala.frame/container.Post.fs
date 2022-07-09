namespace pilipala.container.Post

open System
open fsharper.op.Alias

type IPostMetaEntry =
    /// 元信息id
    abstract metaId : u64
    /// 上级元信息id
    abstract baseMetaId : u64 with get, set
    /// 绑定记录id
    abstract bindRecordId : u64 with get, set
    /// 创建时间
    abstract ctime : DateTime with get, set
    /// 访问时间
    abstract atime : DateTime with get, set
    /// 访问数
    abstract view : u32 with get, set
    /// 星星数
    abstract star : u32 with get, set

type IPostRecordEntry =
    /// 记录id
    abstract recordId : u64
    /// 封面
    abstract cover : string with get, set
    /// 标题
    abstract title : string with get, set
    /// 概述
    abstract summary : string with get, set
    /// 正文
    abstract body : string with get, set
    /// 修改时间
    abstract mtime : DateTime with get, set
