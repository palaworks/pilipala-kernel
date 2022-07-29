namespace pilipala.builder

open Microsoft.Extensions.DependencyInjection
open fsharper.typ.Pipe
open pilipala
open pilipala.log
open pilipala.serv
open pilipala.plugin

type Builder = { pipeline: IPipe<IServiceCollection> }

(*
    内核构造序：
    useDb
    usePlugin
    useAuth
    useLog
    useLog
    useLog
    useService
    useService
    useService
    usePostCache
    useCommentCache
    *)

type Builder with

    member self.build() =
        let f (sc: IServiceCollection) =
            sc
                .AddSingleton<LogProvider>()
                .AddSingleton<ServProvider>()
                .AddSingleton<PluginProvider>()

        StatePipe(activate = f)
            .export(self.pipeline)
            .export(StatePipe(activate = fun sc -> sc.AddSingleton<Pilipala>()))
            .fill(ServiceCollection())
            .BuildServiceProvider()
            .GetService<Pilipala>()
