namespace pilipala.container

/// 无法同步到数据库
exception FailedToSyncDbException

/// 无法创建文章记录
exception FailedToCreatePostRecordException
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

/// 无效Id
exception InvalidIdException