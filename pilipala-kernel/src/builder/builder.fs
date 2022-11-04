namespace pilipala.builder

open System
open System.Threading.Tasks
open fsharper.typ
open fsharper.op.Foldable
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open pilipala
open pilipala.access.user
open pilipala.data.db
open pilipala.id
open pilipala.log
open pilipala.pipeline.user
open pilipala.plugin
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
                .AddSingleton<_>({ Plugins = [] }) //插件注册器
                //ID生成器
                .AddSingleton<IPalaflakeGenerator>(fun _ -> IPalaflakeGenerator.make 01uy)
                .AddSingleton<IUuidGenerator>(fun _ -> IUuidGenerator.make ())
                //文章管道构造器
                .AddSingleton<IPostInitPipelineBuilder>(fun _ -> IPostInitPipelineBuilder.make ())
                .AddSingleton<IPostRenderPipelineBuilder>(fun _ -> IPostRenderPipelineBuilder.make ())
                .AddSingleton<IPostModifyPipelineBuilder>(fun _ -> IPostModifyPipelineBuilder.make ())
                .AddSingleton<IPostFinalizePipelineBuilder>(fun _ -> IPostFinalizePipelineBuilder.make ())
                //文章管道
                .AddTransient<_>(fun sf -> IPostInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf -> IPostRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf -> IPostModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf ->
                    IPostFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射文章提供器
                .AddTransient<_>(fun sf ->
                    IMappedPostProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //评论管道构造器
                .AddSingleton<ICommentInitPipelineBuilder>(fun _ -> ICommentInitPipelineBuilder.make ())
                .AddSingleton<ICommentRenderPipelineBuilder>(fun _ -> ICommentRenderPipelineBuilder.make ())
                .AddSingleton<ICommentModifyPipelineBuilder>(fun _ -> ICommentModifyPipelineBuilder.make ())
                .AddSingleton<ICommentFinalizePipelineBuilder>(fun _ -> ICommentFinalizePipelineBuilder.make ())
                //评论管道
                .AddTransient<_>(fun sf -> ICommentInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf ->
                    ICommentRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf ->
                    ICommentModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf ->
                    ICommentFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射评论提供器
                .AddTransient<_>(fun sf ->
                    IMappedCommentProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //用户管道构造器
                .AddSingleton<IUserInitPipelineBuilder>(fun _ -> IUserInitPipelineBuilder.make ())
                .AddSingleton<IUserRenderPipelineBuilder>(fun _ -> IUserRenderPipelineBuilder.make ())
                .AddSingleton<IUserModifyPipelineBuilder>(fun _ -> IUserModifyPipelineBuilder.make ())
                .AddSingleton<IUserFinalizePipelineBuilder>(fun _ -> IUserFinalizePipelineBuilder.make ())
                //用户管道
                .AddTransient<_>(fun sf ->
                    IUserInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf -> IUserRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf -> IUserModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .AddTransient<_>(fun sf ->
                    IUserFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射用户提供器
                .AddTransient<_>(fun sf ->
                    IMappedUserProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
        .> self.pipeline //use...
        .> fun sc -> //添加已注册日志
            let lr: LoggerRegister =
                sc.BuildServiceProvider().GetRequiredService()

            sc.AddLogging (fun builder ->
                lr.LoggerFilters.foldr
                <| (fun (k, v) (acc: ILoggingBuilder) -> acc.AddFilter(k, v))
                <| builder
                |> ignore

                lr.LoggerProviders.foldr
                <| (fun p (acc: ILoggingBuilder) -> acc.AddProvider p)
                <| builder
                |> ignore)
        //before build
        .> fun sc ->
            sc
                .AddSingleton<IApp, _>(fun sf ->
                    App(
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                .BuildServiceProvider()
        //after build
        .> fun sp -> //启动已注入插件(BeforeBuild
            sp
                .GetRequiredService<PluginRegister>()
                .Plugins
                .foldr
            <| fun (pluginType, hookTime) (acc: IServiceProvider) ->
                if hookTime = AppLifeCycle.BeforeBuild then
                    //在新的容器中限定插件资产以及标识作用域
                    ServiceCollection()
                        .AddSingleton<IDbOperationBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IPluginCfgProvider>(fun _ -> IPluginCfgProvider.make pluginType)
                        //文章构造资产
                        .AddSingleton<IPostInitPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IPostRenderPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IPostModifyPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IPostFinalizePipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddTransient<IPostInitPipeline>(fun _ -> sp.GetRequiredService())
                        .AddTransient<IPostRenderPipeline>(fun _ -> sp.GetRequiredService())
                        .AddTransient<IPostModifyPipeline>(fun _ -> sp.GetRequiredService())
                        .AddTransient<IPostFinalizePipeline>(fun _ -> sp.GetRequiredService())
                        .AddTransient<IMappedPostProvider>(fun _ -> sp.GetRequiredService())
                        //评论构造资产
                        .AddSingleton<ICommentInitPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentRenderPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentModifyPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentFinalizePipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentInitPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentRenderPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentModifyPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<ICommentFinalizePipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IMappedCommentProvider>(fun _ -> sp.GetRequiredService())
                        //用户构造资产
                        .AddSingleton<IUserInitPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserRenderPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserModifyPipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserFinalizePipelineBuilder>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserInitPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserRenderPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserModifyPipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IUserFinalizePipeline>(fun _ -> sp.GetRequiredService())
                        .AddSingleton<IMappedUserProvider>(fun _ -> sp.GetRequiredService())
                        //...
                        .AddSingleton(
                            pluginType
                        )
                        .BuildServiceProvider()
                        .GetRequiredService(pluginType)
                    |> effect (fun _ -> //记录到日志
                        sp
                            .GetRequiredService<ILogger<Builder>>()
                            .LogInformation($"Plugin loaded: {pluginType.Name}"))
                else
                    ()
                |> always acc
            <| sp
        .> fun sp ->
            sp.GetRequiredService<IApp>()
            |> effect (fun app ->
                sp
                    .GetRequiredService<PluginRegister>()
                    .Plugins
                    .foldr
                <| fun (pluginType, hookTime) (acc: IServiceProvider) ->
                    if hookTime = AppLifeCycle.AfterBuild then
                        //在新的容器中限定插件资产以及标识作用域
                        ServiceCollection()
                            .AddSingleton<IApp>(fun _ -> app)
                            .AddSingleton<IDbOperationBuilder>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IPluginCfgProvider>(fun _ -> IPluginCfgProvider.make pluginType)
                            //文章管道资产
                            .AddTransient<IPostInitPipeline>(fun _ -> sp.GetRequiredService())
                            .AddTransient<IPostRenderPipeline>(fun _ -> sp.GetRequiredService())
                            .AddTransient<IPostModifyPipeline>(fun _ -> sp.GetRequiredService())
                            .AddTransient<IPostFinalizePipeline>(fun _ -> sp.GetRequiredService())
                            .AddTransient<IMappedPostProvider>(fun _ -> sp.GetRequiredService())
                            //评论管道资产
                            .AddSingleton<ICommentInitPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<ICommentRenderPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<ICommentModifyPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<ICommentFinalizePipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IMappedCommentProvider>(fun _ -> sp.GetRequiredService())
                            //用户管道资产
                            .AddSingleton<IUserInitPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IUserRenderPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IUserModifyPipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IUserFinalizePipeline>(fun _ -> sp.GetRequiredService())
                            .AddSingleton<IMappedUserProvider>(fun _ -> sp.GetRequiredService())
                            //...
                            .AddSingleton(
                                pluginType
                            )
                            .BuildServiceProvider()
                            .GetRequiredService(pluginType)
                        |> effect (fun _ -> //记录到日志
                            sp
                                .GetRequiredService<ILogger<Builder>>()
                                .LogInformation($"Plugin loaded: {pluginType.Name}"))
                        |> ignore

                    acc
                <| sp)
        |> apply (ServiceCollection())
