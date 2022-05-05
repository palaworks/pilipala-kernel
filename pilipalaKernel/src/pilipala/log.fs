module pilipala.log

open System
open System.IO
open System.Collections.Generic
open fsharper.typ.Pipe.Pipable
open pilipala.util.stream

/// 生成日志流
let mutable internal genLogStream: unit -> Stream = fun () -> Stream.Null
