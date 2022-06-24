namespace pilipala.pipeline.comment

open System
open fsharper.op.Alias
open fsharper.typ.Pipe.Pipable
open pilipala.pipeline

[<AutoOpen>]
module get =
    let nick = ref (Pipe<string>())
    let content = ref (Pipe<string>())
    let email = ref (Pipe<string>())
    let site = ref (Pipe<string>())
    let ctime = ref (Pipe<DateTime>())

[<AutoOpen>]
module typ =
    type CommentRenderPipeline internal () =
        let gen (p: Ref<Pipe<'t>>) =
            { new IRenderPipeLine<'t> with
                member i.Before(pipe: Pipe<'t>) =
                    p.Value <- p.Value.import pipe
                    i

                member i.Then(pipe: Pipe<'t>) =
                    p.Value <- p.Value.mappend pipe
                    i }

        member self.nick = gen nick
        member self.content = gen content
        member self.email = gen email
        member self.site = gen site
        member self.ctime = gen ctime
