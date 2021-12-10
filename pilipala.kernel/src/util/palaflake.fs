namespace pilipala.util

module palaflake =

    open System
    open System.Threading
    open System.Diagnostics
    open fsharper.moreType

    type Generator(machineId: byte, startYear: uint16) =
        //暂不考虑State Monad

        let start =
            Trace.Assert(DateTime.UtcNow.Year >= int startYear, "The start_year cannot be set to a future time")
            Trace.Assert(uint16 DateTime.UtcNow.Year - startYear < 34us, "The startYear cannot older than 34 years")
            DateTime(int startYear, 1, 1, 0, 0, 0, DateTimeKind.Utc)

        let mutable lastTimestamp = 0UL

        let mutable cb = 0uy //回拨次数
        let mutable seq = 0us //序列号

        let mutex = Object()

        //ID结构参考
        //11111111 00223333 33333333 33333333
        //33333333 33333333 33334444 44444444

        member this.Next() =
            lock mutex
            <| fun _ ->
                let utc = DateTime.UtcNow

                //当前时间早于起始时间
                Trace.Assert(utc > start, "Abnormal system time")
                let mutable currTimestamp = uint64 (utc - start).TotalMilliseconds

                match cmp currTimestamp lastTimestamp with
                | GT -> seq <- 0us
                | EQ ->
                    seq <- seq + 1us

                    if seq = 4096us //一毫秒内的请求超过4096次
                    then
                        Thread.Sleep 1 //阻塞一毫秒
                        currTimestamp <- currTimestamp + 1UL
                        seq <- 0us

                | LT ->
                    cb <- cb + 1uy
                    Trace.Assert(cb < 4uy, "Too many clock adjustments") //超出了最大回拨次数

                lastTimestamp <- currTimestamp

                (uint64 machineId <<< 56)
                ||| (uint64 cb <<< 52)
                ||| (currTimestamp <<< 12)
                ||| uint64 seq


    let private g = Generator(1uy, 2021us)
    /// 生成palaflake
    let gen () = g.Next()
