namespace pilipala.container

[<AutoOpen>]
module err =

    /// 无法写缓存错误
    /// 这可能是由id错误或数据库连接存在问题导致的
    exception FailedToWriteCache

    /// 无法创建文章记录
    exception FailedToCreateRecord
    /// 无法抹除文章记录
    exception FailedToEraseRecord

    /// 无法创建文章栈
    exception FailedToCreateStack
    /// 无法抹除文章栈
    exception FailedToEraseStack

    /// 无法创建评论
    exception FailedToCreateComment
    /// 无法抹除评论
    exception FailedToEraseComment
