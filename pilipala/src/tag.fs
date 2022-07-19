module pilipala.container.tag

open fsharper.op
open fsharper.typ
open fsharper.typ.Ord
open fsharper.op.Alias
open DbManaged
open DbManaged.PgSql

open pilipala
open pilipala.data.db
open pilipala.container.post

/// 无法创建标签错误
exception FailedToCreateTag
/// 无法抹除标签错误
exception FailedToEraseTag
/// 无法加标签错误
exception FailedToTag
/// 无法去标签错误
exception FailedToDetag


/// 标签本质上是metaId的列表
/// 可以根据该列表过滤出不同的文章

/// 标签别名
type Tag = u64 list

type internal TagProvider(dp: IDbProvider) =

    /// 创建标签
    /// 返回被创建标签名
    member self.create(tagName: string) =
        if tagName = "" then
            Err FailedToCreateTag //标签名不能为空
        else
            let sql =
                $"CREATE TABLE tag_{tagName} \
                              (metaId BIGINT PRIMARY KEY NOT NULL)"

            let aff =

                dp.mkCmd().query(sql).whenEq (0)
                |> dp.managed.executeQuery

            if aff |> eq 0 then
                tagName.ToLower() |> Ok
            else
                Err FailedToCreateTag

    /// 抹除标签
    member self.erase(tagName: string) =

        let sql = $"DROP TABLE tag_{tagName}"

        let aff =
            dp.mkCmd().query(sql).alwaysCommit ()
            |> dp.managed.executeQuery

        if aff |> eq 0 then
            Ok()
        else
            Err FailedToEraseTag

    /// 为文章元加标签
    member self.tagTo (metaId: u64) (tagName: string) =

        let sql =
            $"INSERT INTO tag_{tagName} (metaId) VALUES (<metaId>)"
            |> dp.managed.normalizeSql

        let paras: (string * obj) list = [ ("metaId", metaId) ]

        let aff =
            dp.mkCmd().query(sql, paras).whenEq (1)
            |> dp.managed.executeQuery

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToTag

    /// 为文章元去除标签
    member self.detagFor (metaId: u64) (tagName: string) =
        let aff =
            dp
                .mkCmd()
                .delete($"tag_{tagName}", "metaId", metaId)
                .whenEq (1)
            |> dp.managed.executeQuery

        if aff |> eq 1 then
            Ok()
        else
            Err FailedToDetag

    /// 取得标签
    member self.getTag(tagName: string) =

        let sql = $"SELECT metaId FROM tag_{tagName}"

        let list =
            dp.mkCmd().getFstCol (sql)
            |> dp.managed.executeQuery

        Ok [ for x in list -> x :?> u64 ]

    /// 过滤出是 tag 的文章
    member self.is (tag: Tag) (ps: Post list) =
        ps |> filter (fun p -> elem p.postId tag)

    /// 过滤出不是 tag 的文章
    member self.not (tag: Tag) (ps: Post list) =
        ps |> filter (fun p -> not <| elem p.postId tag)
