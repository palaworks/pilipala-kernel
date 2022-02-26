﻿module pilipala.container.tag

open MySqlManaged
open MySql.Data.MySqlClient
open fsharper.fn
open fsharper.op
open fsharper.ethType
open fsharper.typeExt
open fsharper.moreType
open pilipala
open pilipala.container.post

/// 无法创建标签错误
exception FailedToCreateTag
/// 无法抹除标签错误
exception FailedToEraseTag
/// 无法加标签错误
exception FailedToTag
/// 无法去标签错误
exception FailedToDetag


/// 标签本质上是stackId的列表
/// 可以根据该列表过滤出不同的文章

/// 标签别名
type Tag = uint64 list

/// 创建标签
/// 返回被创建标签名
let create (tagName: string) =
    if tagName = "" then
        Err FailedToCreateTag //标签名不能为空
    else
        let sql =
            $"CREATE TABLE tag_{tagName} \
                          (stackId BIGINT PRIMARY KEY NOT NULL)"

        schema.Managed().execute sql
        >>= fun f ->
                match f <| eq 0 with
                | 0 -> tagName.ToLower() |> Ok
                | _ -> Err FailedToCreateTag

/// 抹除标签
let erase (tagName: string) =

    let sql = $"DROP TABLE tag_{tagName}"

    schema.Managed().execute sql
    >>= fun f ->
            match (fun _ -> true) |> f with
            | 0 -> Ok()
            | _ -> Err FailedToEraseTag

/// 为文章栈加标签
let tagTo (stackId: uint64) (tagName: string) =

    let sql =
        $"INSERT INTO tag_{tagName} (stackId) VALUES (?stackId)"

    let para = [| MySqlParameter("stackId", stackId) |]

    schema.Managed().execute (sql, para)
    >>= fun f ->
            match f <| eq 1 with
            | 1 -> Ok()
            | _ -> Err FailedToTag

/// 为文章栈去除标签
let detagFor (stackId: uint64) (tagName: string) =

    schema.Managed().executeDelete $"tag_{tagName}" ("stackId", stackId)
    >>= fun f ->
            match f <| eq 1 with
            | 1 -> Ok()
            | _ -> Err FailedToDetag

/// 取得标签
let getTag (tagName: string) =

    let sql = $"SELECT stackId FROM tag_{tagName}"

    schema.Managed().getFstCol sql
    >>= fun r ->
            Ok
            <| match r with
               | Some list -> [ for x in list -> x :?> uint64 ]
               | None -> []

/// 过滤出是 tag 的文章
let is (tag: Tag) (ps: PostMeta list) =
    ps |> filter (fun p -> elem p.stackId tag)

/// 过滤出不是 tag 的文章
let not (tag: Tag) (ps: PostMeta list) =
    ps |> filter (fun p -> not <| elem p.stackId tag)
