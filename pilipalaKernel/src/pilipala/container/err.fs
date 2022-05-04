namespace pilipala.container

/// 无法写缓存错误
/// 这可能是由id错误或数据库连接存在问题导致的
exception FailedToWriteCacheException

/// 无法创建文章记录
exception FailedToCreateRecordException
/// 无法抹除文章记录
exception FailedToEraseRecordException

/// 无法创建文章元
exception FailedToCreateMetaException
/// 无法抹除文章元
exception FailedToEraseMetaException

/// 无法创建评论
exception FailedToCreateCommentException
/// 无法抹除评论
exception FailedToEraseCommentException
