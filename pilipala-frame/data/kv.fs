namespace pilipala.data.kv

open fsharper.typ

type IKvProvider =

    /// 获取键值
    abstract get: 'k -> Option'<'v>
    /// 设置或更新键值
    abstract set: 'k -> 'v -> unit
    /// 删除键值
    abstract del: 'k -> unit
    /// 删除所有键值
    abstract clear: unit -> unit
    /// 判断键值是否存在
    abstract exists: 'k -> bool
