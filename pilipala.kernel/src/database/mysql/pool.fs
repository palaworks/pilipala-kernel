namespace pilipala.database.mysql

open System.Data
open MySql.Data.MySqlClient
open pilipala.data

/// MySql数据库连接池
type private MySqlConnPool
    (
        msg: {| DataSource: string
                Port: uint16
                User: string
                Password: string |},
        schema,
        size: uint
    ) =

    /// 连接列表
    /// 新连接总是在此列表首部
    let mutable ConnList: MySqlConnection list = []

    /// 尝试清理连接池
    let tryCleanConnPool () =
        ConnList <-
            filter
            <| fun (conn: MySqlConnection) ->
                match conn.State with
                //如果连接中断或是关闭（这都是不工作的状态）
                | ConnectionState.Broken
                | ConnectionState.Closed ->
                    conn.Dispose() //注销
                    false //移除
                | _ -> true //保留
            <| ConnList


    /// 取得空闲连接
    let getIdleConn () =
        filter
        <| fun (conn: MySqlConnection) ->
            match conn.State with
            | ConnectionState.Broken
            | ConnectionState.Closed ->
                conn.Dispose()
                false
            | _ -> true

        <| ConnList

        |> head

    /// 连接字符串
    let ConnStr =
        if schema = "" then//TODO 等号右侧空格测试
            $"DataSource ={msg.DataSource};\
                    Port ={msg.Port};\
                  UserID ={msg.User};\
                Password ={msg.Password};\
         UseAffectedRows =TRUE;" //使UPDATE语句返回受影响的行数而不是符合查询条件的行数
        else
            $"DataSource ={msg.DataSource};\
                DataBase ={schema};\
                    Port ={msg.Port};\
                  UserID ={msg.User};\
                Password ={msg.Password};\
         UseAffectedRows =TRUE;"

    /// 从连接池取用 MySqlConnection
    member this.getConnection() =
        let genConn () =
            let newConn = new MySqlConnection(ConnStr)

            ConnList <- newConn :: ConnList

            newConn.Open()
            newConn

        try
            Ok
            <| match uint ConnList.Length with
               | len when //连接数较少时，新建
                   len <= size / 2u
                   ->
                   genConn ()
               | len when //连接数较多时，在循环复用的基础上新建
                   len <= size
                   ->
                   match getIdleConn () with
                   | Some c -> c
                   | None -> genConn ()
               | _ -> //连接数过多时，清理后新建
                   tryCleanConnPool ()
                   genConn ()

        with
        | e -> Err e


type MySqlConnPool with

    /// 创建一个 MySqlConnection, 并以其为参数执行闭包 f
    /// MySqlConnection 销毁权交由闭包 f
    member self.useConnection f =
        self.getConnection () >>= fun conn -> f conn |> Ok

    /// 托管一个 MySqlConnection, 并以其为参数执行闭包 f
    /// 闭包执行完成后该 MySqlConnection 会被销毁
    member self.hostConnection f =
        self.useConnection
        <| fun conn ->
            let result = f conn
            conn.Dispose()
            result
