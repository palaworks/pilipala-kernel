namespace pilipala.access.user

open System
open fsharper.typ
open fsharper.op.Alias

type IUser =
    abstract Id: u64
    abstract Name: string with get, set
    //abstract PwdHash: string with get, set
    abstract Email: string with get, set
    abstract Permission: u16 with get, set
    abstract CreateTime: DateTime with get, set
    abstract AccessTime: DateTime with get, set
    abstract Item: string -> Option'<obj> with get, set

type IUserProvider =
    abstract fetch: u64 -> IUser
    abstract create: IUser -> u64
    abstract delete: u64 -> u64 * IUser

type LoginData = { userName: string; userPwd: string }
