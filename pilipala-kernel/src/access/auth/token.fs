namespace pilipala.access.auth.token

open System
open fsharper.typ
open pilipala.data.db
open pilipala.id
open pilipala.util.hash

//TODO：应使用随机化IV+CBC以代替ECB模式以获得最佳安全性
type internal TokenProvider(db: IDbOperationBuilder, uuid: IUuidGenerator) =

    let tokenHasher (s: string) = s.bcrypt

    /// 创建凭据
    /// 返回凭据值
    member self.create(expire_time: DateTime) =
        let token = uuid.next ()

        let fields: (_ * obj) list =
            [ ("token_hash", tokenHasher token)
              ("token_create_time", DateTime.Now)
              ("token_access_time", DateTime.Now)
              ("token_expire_time", expire_time) ]

        let aff =
            db {
                inToken
                insert fields
                whenEq 1
                execute
            }

        if aff |> eq 1 then
            Ok token
        else
            Err $"Failed to create token (aff {aff})"

    /// 删除凭据
    member self.delete(token_hash: string) =
        let aff =
            db {
                inToken
                delete "token_hash" token_hash
                whenEq 1
                execute
            }

        if aff |> eq 1 then
            Ok()
        else
            Err $"Failed to delete token (aff {aff})"

    /// 检查token是否合法
    member self.check(token_hash: string) =
        //TODO 貌似sql也可以采用消除基本类型偏执的规格化思路
        let sql =
            $"SELECT COUNT(*) FROM {db.tables.token} WHERE tokenHash = :tokenHash"

        let n =
            db {
                getFstVal sql [ ("tokenHash", token_hash) ]
                execute
            }

        match n with
        //如果查询到的凭据记录唯一
        | Some x when x = 1 ->
            //更新凭据访问记录
            let aff =
                db {
                    inToken
                    update "token_access_time" DateTime.Now "token_hash" token_hash
                    whenEq 1
                    execute
                }

            eq aff 1 //无法更新访问时间，也视为非法
        | _ -> false //重复、查不到均视为非法
