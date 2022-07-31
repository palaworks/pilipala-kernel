[<AutoOpen>]
[<RequireQualifiedAccess>]
module pilipala.util.id.palaflake

open System
open System.Threading
open fsharper.op.Alias

type Generator(instanceId: u8, startYear: u16) =

    let start =
        //设置了未来时间
        if DateTime.UtcNow.Year < i32 startYear then
            failwith $"The startYear({startYear}) cannot be set to a future time"
        //时间戳溢出
        if u16 DateTime.UtcNow.Year - startYear >= 34us then
            failwith $"The startYear({startYear}) cannot older than 34 years"

        DateTime(i32 startYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)

    let instanceId = i64 instanceId
    let mutable lastTimestamp = 0L
    let mutable cb = 0L //回拨次数
    let mutable seq = 0L //序列号

    //ID结构参考
    //01112222 22222222 22222222 22222222
    //22222222 22223333 33334444 44444444

    /// 生成ID
    member self.Next() : i64 =
        fun _ ->
            let utcNow = DateTime.UtcNow

            if utcNow < start then //当前时间早于起始时间
                failwith $"Illegal system time({utcNow})"

            let mutable currTimestamp =
                i64 (utcNow - start).TotalMilliseconds

            if currTimestamp > lastTimestamp then
                seq <- 0L
            elif currTimestamp = lastTimestamp then
                seq <- seq + 1L

                if seq > 4095L then //一毫秒内的请求超过4096次
                    Thread.Sleep 1 //阻塞一毫秒
                    currTimestamp <- currTimestamp + 1L
                    seq <- 0L
            else //LT，发生时间回拨
                cb <- cb + 1L

                if cb > 7L then //超出了最大回拨次数
                    failwith $"Out of max clock adjustments({cb})"

            lastTimestamp <- currTimestamp

            (cb <<< 60)
            ||| (currTimestamp <<< 20)
            ||| (instanceId <<< 12)
            ||| seq

        |> lock self
