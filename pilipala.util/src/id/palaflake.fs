[<AutoOpen>]
module pilipala.util.id.palaflake

open System
open System.Threading
open fsharper.op.Alias
open fsharper.typ.Ord

type Generator(machineId: u8, startYear: u16) =
    //暂不考虑State Monad

    let start =
        if DateTime.UtcNow.Year < i32 startYear then
            failwith "The startYear cannot be set to a future time"

        if u16 DateTime.UtcNow.Year - startYear >= 34us then
            failwith "The startYear cannot older than 34 years"

        DateTime(i32 startYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)

    let mutable lastTimestamp = 0uL

    let mutable cb = 0uy //回拨次数
    let mutable seq = 0us //序列号

    //ID结构参考
    //11111111 00223333 33333333 33333333
    //33333333 33333333 33334444 44444444

    member self.Next() =
        fun _ ->
            let utcNow = DateTime.UtcNow

            if utcNow < start then //当前时间早于起始时间
                failwith "Abnormal system time"

            let mutable currTimestamp =
                u64 (utcNow - start).TotalMilliseconds

            match cmp currTimestamp lastTimestamp with
            | GT -> seq <- 0us
            | EQ ->
                seq <- seq + 1us

                if seq >= 4096us then //一毫秒内的请求超过4096次
                    Thread.Sleep 1 //阻塞一毫秒
                    currTimestamp <- currTimestamp + 1uL
                    seq <- 0us

            | LT ->
                cb <- cb + 1uy

                if cb >= 4uy then //超出了最大回拨次数
                    failwith "Too many clock adjustments"

            lastTimestamp <- currTimestamp

            (u64 machineId <<< 56)
            ||| (u64 cb <<< 52)
            ||| (currTimestamp <<< 12)
            ||| u64 seq

        |> lock self
