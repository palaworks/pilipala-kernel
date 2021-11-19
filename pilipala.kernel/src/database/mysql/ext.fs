namespace pilipala.database.mysql

[<AutoOpen>]
module ext =

    open MySql.Data.MySqlClient
    open pilipala.data

    type MySqlCommand with

        /// 创建一个 MySqlTransaction, 并以其为参数执行闭包 f
        /// MySqlTransaction 销毁权交由闭包 f
        member self.useTransaction f =
            let tx = self.Connection.BeginTransaction()
            self.Transaction <- tx
            f tx


        /// 托管一个 MySqlTransaction, 并以其为参数执行闭包 f
        /// 闭包执行完成后该 MySqlTransaction 会被销毁
        member self.hostTransaction f =
            self.useTransaction
            <| fun tx ->
                let result = f tx
                tx.Dispose()
                result


    type MySqlConnection with

        /// 创建一个 MySqlCommand, 并以其为参数执行闭包 f
        /// MySqlCommand 销毁权交由闭包 f
        member self.useCommand f =
            let cmd = self.CreateCommand()
            f cmd

        /// 托管一个 MySqlCommand, 并以其为参数执行闭包 f
        /// 闭包执行完成后该 MySqlCommand 会被销毁
        member self.hostCommand f =
            self.useCommand
            <| fun cmd ->
                let result = f cmd
                cmd.Dispose()
                result


    type MySqlConnection with

        /// 执行任意查询
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.execute sql =
            self.useCommand
            <| fun cmd ->
                cmd.CommandText <- sql

                cmd.useTransaction
                <| fun tx ->
                    fun p ->
                        let affected =
                            match cmd.ExecuteNonQuery() with
                            | n when p n -> //符合期望影响行数规则则提交
                                tx.Commit()
                                n
                            | _ -> //否则回滚
                                tx.Rollback()
                                0

                        tx.Dispose() //资源释放
                        cmd.Dispose()

                        affected //实际受影响的行数

        /// 执行任意参数化查询
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.execute(sql, para) =
            self.useCommand
            <| fun cmd ->
                cmd.CommandText <- sql
                cmd.Parameters.AddRange para //添加参数

                cmd.useTransaction
                <| fun tx ->
                    fun p ->
                        let affected =
                            match cmd.ExecuteNonQuery() with
                            | n when p n -> //符合期望影响行数规则则提交
                                tx.Commit()
                                n
                            | _ -> //否则回滚
                                tx.Rollback()
                                0

                        tx.Dispose() //资源释放
                        cmd.Dispose()

                        affected //实际受影响的行数

        /// 将 table 中 whereKey 等于 whereKeyVal 的行的 setKey 更新为 setKeyVal
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.executeUpdate(table: string, (setKey: string, setKeyVal), (whereKey: string, whereKeyVal)) =
            self.useCommand
            <| fun cmd ->
                cmd.CommandText <-
                    $"UPDATE `{table}` \
                         SET `{setKey}`=?setKeyVal \
                       WHERE `{whereKey}`=?whereKeyVal"

                cmd.Parameters.AddWithValue("setKeyVal", setKeyVal)
                |> ignore

                cmd.Parameters.AddWithValue("whereKeyVal", whereKeyVal)
                |> ignore

                cmd.useTransaction
                <| fun tx ->
                    fun p ->
                        let affected =
                            match cmd.ExecuteNonQuery() with
                            | n when p n -> //符合期望影响行数规则则提交
                                tx.Commit()
                                n
                            | _ -> //否则回滚
                                tx.Rollback()
                                0

                        tx.Dispose() //资源释放
                        cmd.Dispose()

                        affected //实际受影响的行数

        /// 将 table 中 key 等于 oldValue 的行的 key 更新为 newValue
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.executeUpdate(table, key, newValue: 'V, oldValue: 'V) =
            (table, (key, newValue), (key, oldValue))
            |> self.executeUpdate

        /// 在 table 中插入一行
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.executeInsert (table: string) pairs =
            self.useCommand
            <| fun cmd ->

                let x =
                    pairs
                    |> foldl
                        (fun (a, b) (k, v) ->

                            cmd.Parameters.AddWithValue(k, v) //添加参数
                            |> ignore

                            //a 为VALUES语句前半部分
                            //b 为VALUES语句后半部分
                            (a + $"`{k}`,", b + $"?{v} ,"))
                        ("", "")

                cmd.CommandText <-
                    $"INSERT INTO `{table}` \
                      ({(fst x).[0..^1]}) \
                      VALUES \
                      ({(snd x).[0..^1]})"

                cmd.useTransaction
                <| fun tx ->
                    fun p ->
                        let affected =
                            match cmd.ExecuteNonQuery() with
                            | n when p n -> //符合期望影响行数规则则提交
                                tx.Commit()
                                n
                            | _ -> //否则回滚
                                tx.Rollback()
                                0

                        tx.Dispose() //资源释放
                        cmd.Dispose()

                        affected //实际受影响的行数

        /// 删除 table 中 whereKey 等于 whereKeyVal 的行
        /// 返回的闭包用于检测受影响的行数，当断言成立时闭包会提交事务并返回受影响的行数
        member self.executeDelete (table: string) (whereKey: string, whereKeyVal) =
            self.useCommand
            <| fun cmd ->
                cmd.CommandText <- $"DELETE FROM `{table}` WHERE `{whereKey}`=?Value"

                cmd.Parameters.AddWithValue("Value", whereKeyVal) //添加参数
                |> ignore

                cmd.useTransaction
                <| fun tx ->
                    fun p ->
                        let affected =
                            match cmd.ExecuteNonQuery() with
                            | n when p n -> //符合期望影响行数规则则提交
                                tx.Commit()
                                n
                            | _ -> //否则回滚
                                tx.Rollback()
                                0

                        tx.Dispose() //资源释放
                        cmd.Dispose()

                        affected //实际受影响的行数
