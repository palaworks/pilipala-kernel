module pilipala.util.stream

open System
open System.IO

/// 流分发器
/// 向该流的写入会被分发到所有流上
type StreamDistributor(streams: Stream array) =

    override self.Flush() =

        for s in streams do
            base.WriteTo s
            s.Flush()

        base.SetLength(0)

    /// 将所有对分发器的写入分发到所有流上，然后关闭这些流和分发器流
    override self.Close() =
        for s in streams do
            base.WriteTo s
            s.Close()

        base.Close()
