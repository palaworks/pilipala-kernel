[<AutoOpen>]
module pilipala.access.user.ext

type IUser with

    member self.ReadPostPermissionLevel = self.Permission &&& 3072us >>> 10

    member self.WritePostPermissionLevel = self.Permission &&& 300us >>> 8

    member self.ReadCommentPermissionLevel = self.Permission &&& 192us >>> 6

    member self.WriteCommentPermissionLevel = self.Permission &&& 48us >>> 4

    member self.ReadUserPermissionLevel = self.Permission &&& 12us >>> 2

    member self.WriteUserPermissionLevel = self.Permission &&& 3us

type IUser with

    member self.CanReadPost(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 3072us) > (postUserPermission &&& 3072us) then
            true //凌驾关系
        else
            false

    member self.CanWritePost(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 300us) > (postUserPermission &&& 300us) then
            true //凌驾关系
        else
            false

    member self.CanReadComment(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 192us) > (postUserPermission &&& 192us) then
            true //凌驾关系
        else
            false

    member self.CanWriteComment(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 48us) > (postUserPermission &&& 48us) then
            true //凌驾关系
        else
            false

    member self.CanReadUser(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 12us) > (postUserPermission &&& 12us) then
            true //凌驾关系
        else
            false

    member self.CanWriteUser(postUserPermission, postUserId) =
        if self.Id = postUserId then
            true //固有资产
        elif (self.Permission &&& 3us) > (postUserPermission &&& 3us) then
            true //凌驾关系
        else
            false
