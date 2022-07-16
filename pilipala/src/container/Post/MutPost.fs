namespace pilipala.container.Post

open fsharper.op.Alias
open pilipala.container.Post
open System


type MutPost internal (postId: u64) =

    
    member self.Body
        with get():string
    /// 创建时间
    /// 此时间不可自定义，由pilipala托管
    member self.CreateTime
        with get (): DateTime = meta_entry.ctime
        and set (v: DateTime) = meta_entry.ctime <- v

    /// 访问时间
    member self.AccessTime
        with get (): DateTime = meta_entry.atime
        and set (v: DateTime) = meta_entry.atime <- v

    /// 修改时间
    /// 此时间不可自定义，由pilipala托管
    member self.ModifyTime = record_entry.mtime

    (*
    /// 访问数
    member self.view
        with get (): u32 = meta_entry.view
        and set (v: u32) = meta_entry.view <- v
    /// 星星数
    member self.star
        with get (): u32 = meta_entry.star
        and set (v: u32) = meta_entry.star <- v

    /// 封面
    member self.cover
        with get (): string = record_entry.cover
        and set (v: string) = record_entry.cover <- v
    /// 标题
    member self.title
        with get (): string = record_entry.title
        and set (v: string) = record_entry.title <- v
    /// 概述
    member self.summary
        with get (): string = record_entry.summary
        and set (v: string) = record_entry.summary <- v
    /// 正文
    member self.body
        with get (): string = record_entry.body
        and set (v: string) = record_entry.body <- v
    /// 文章id
    /// 此项目不可自定义，由pilipala托管
    member self.postId = postId
*)
