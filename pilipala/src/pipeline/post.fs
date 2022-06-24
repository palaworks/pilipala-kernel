namespace pilipala.pipeline.post

open System
open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open pilipala.pipeline

[<AutoOpen>]
module get =
    let cover = ref (Pipe<string>())
    let title = ref (Pipe<string>())
    let summary = ref (Pipe<string>())
    let body = ref (Pipe<string>())
    let ctime = ref (Pipe<DateTime>())
    let mtime = ref (Pipe<DateTime>())
    let atime = ref (Pipe<DateTime>())
    let view = ref (Pipe<u32>())
    let star = ref (Pipe<u32>())

[<AutoOpen>]
module typ =

    type PostRenderPipeline internal () =
        let gen (p: Ref<Pipe<'t>>) =
            { new IRenderPipeLine<'t> with
                member i.Before(pipe: Pipe<'t>) =
                    p.Value <- p.Value.import pipe
                    i

                member i.Then(pipe: Pipe<'t>) =
                    p.Value <- p.Value.mappend pipe
                    i }

        member self.cover = gen cover
        member self.title = gen title
        member self.summary = gen summary
        member self.body = gen body
        member self.ctime = gen ctime
        member self.mtime = gen mtime
        member self.atime = gen atime
        member self.view = gen view
        member self.star = gen star
