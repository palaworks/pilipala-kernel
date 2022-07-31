namespace pilipala.user

open fsharper.op.Alias

type IUser =
    abstract Id: u8

    abstract Permission:
        {| ReadPost: u8
           WritePost: u8
           ReadComment: u8
           WriteComment: u8
           ReadUser: u8
           WriteUser: u8 |}
