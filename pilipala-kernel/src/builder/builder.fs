namespace pilipala.builder

open System
open System.Threading.Tasks
open fsharper.typ
open fsharper.op.Foldable
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open pilipala
open pilipala.access.user
open pilipala.id
open pilipala.log
open pilipala.pipeline.user
open pilipala.plugin
open pilipala.service
open pilipala.service.host
open pilipala.pipeline.post
open pilipala.container.post
open pilipala.pipeline.comment
open pilipala.container.comment

type Builder =
    { pipeline: IServiceCollection -> IServiceCollection }

module Builder =
    let make () = { pipeline = id }

(*
构造顺序：
internal
db
logging
plugin
service
serveOn
*)

type Builder with

    member self.build() =
        fun (sc: IServiceCollection) ->
            sc
                .AddSingleton<_>( //日志注册器
                    { LoggerProviders = []
                      LoggerFilters = [] }
                )
                .AddSingleton<_>({ PluginTypes = [] }) //插件注册器
                .AddSingleton<_>({ ServiceInfos = [] }) //服务注册器
                .AddSingleton<_>({ runAsync = always Task.CompletedTask }) //空的服务主机启动器
                //ID生成器
                .AddSingleton<IPalaflakeGenerator>(fun _ -> IPalaflakeGenerator.make 01uy)
                .AddSingleton<IUuidGenerator>(fun _ -> IUuidGenerator.make ())
                //文章管道构造器
                .AddSingleton<IPostInitPipelineBuilder>(fun _ -> IPostInitPipelineBuilder.make ())
                .AddSingleton<IPostRenderPipelineBuilder>(fun _ -> IPostRenderPipelineBuilder.make ())
                .AddSingleton<IPostModifyPipelineBuilder>(fun _ -> IPostModifyPipelineBuilder.make ())
                .AddSingleton<IPostFinalizePipelineBuilder>(fun _ -> IPostFinalizePipelineBuilder.make ())
                //文章管道
                .AddTransient<_>(fun sf ->
                    IPostInitPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    IPostRenderPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    IPostModifyPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    IPostFinalizePipeline.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                //映射文章提供器
                .AddTransient<_>(fun sf ->
                    IMappedPostProvider.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                //评论管道构造器
                .AddSingleton<ICommentInitPipelineBuilder>(fun _ -> ICommentInitPipelineBuilder.make ())
                .AddSingleton<ICommentRenderPipelineBuilder>(fun _ -> ICommentRenderPipelineBuilder.make ())
                .AddSingleton<ICommentModifyPipelineBuilder>(fun _ -> ICommentModifyPipelineBuilder.make ())
                .AddSingleton<ICommentFinalizePipelineBuilder>(fun _ -> ICommentFinalizePipelineBuilder.make ())
                //评论管道
                .AddTransient<_>(fun sf ->
                    ICommentInitPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    ICommentRenderPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    ICommentModifyPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    ICommentFinalizePipeline.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                //映射评论提供器
                .AddTransient<_>(fun sf ->
                    IMappedCommentProvider.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                //用户管道构造器
                .AddSingleton<IUserInitPipelineBuilder>(fun _ -> IUserInitPipelineBuilder.make ())
                .AddSingleton<IUserRenderPipelineBuilder>(fun _ -> IUserRenderPipelineBuilder.make ())
                .AddSingleton<IUserModifyPipelineBuilder>(fun _ -> IUserModifyPipelineBuilder.make ())
                .AddSingleton<IUserFinalizePipelineBuilder>(fun _ -> IUserFinalizePipelineBuilder.make ())
                //用户管道
                .AddTransient<_>(fun sf ->
                    IUserInitPipeline.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                .AddTransient<_>(fun sf ->
                    IUserRenderPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    IUserModifyPipeline.make (sf.GetRequiredService<_>(), sf.GetRequiredService<_>()))
                .AddTransient<_>(fun sf ->
                    IUserFinalizePipeline.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                //映射用户提供器
                .AddTransient<_>(fun sf ->
                    IMappedUserProvider.make (
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
        .> self.pipeline //use...
        .> fun sc -> //添加已注册日志
            let lr: LoggerRegister =
                sc.BuildServiceProvider().GetRequiredService<_>()

            sc.AddLogging (fun builder ->
                lr.LoggerFilters.foldr
                <| (fun (k, v) (acc: ILoggingBuilder) -> acc.AddFilter(k, v))
                <| builder
                |> ignore

                lr.LoggerProviders.foldr
                <| (fun p (acc: ILoggingBuilder) -> acc.AddProvider p)
                <| builder
                |> ignore)
        .> fun sc -> //注入已注册插件
            sc.BuildServiceProvider().GetRequiredService<PluginRegister>()
                .PluginTypes
                .foldr
            <| fun pluginType (acc: IServiceCollection) -> acc.AddSingleton(pluginType)
            <| sc
        //before build
        .> fun sc ->
            sc
                .AddSingleton<Pilipala>(fun sf ->
                    Pilipala(
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>(),
                        sf.GetRequiredService<_>()
                    ))
                .BuildServiceProvider()
        //after build
        .> fun sp -> //启动已注入插件
            sp
                .GetRequiredService<PluginRegister>()
                .PluginTypes
                .foldr
            <| fun pluginType (acc: IServiceProvider) -> acc.GetRequiredService(pluginType) |> always acc
            <| sp
        .> fun sp -> //启动服务主机
            sp
                .GetRequiredService<RunServiceHost>()
                .runAsync sp
            |> always sp
        .> fun sp -> sp.GetRequiredService<Pilipala>()
        |> apply (ServiceCollection())
