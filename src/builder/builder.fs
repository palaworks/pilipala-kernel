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

module ext_IServiceCollection =
    type IServiceCollection with

        member self.addTransient(f: _ -> 't) = self.AddTransient<'t> f
        member self.addScoped(f: _ -> 't) = self.AddScoped<'t> f
        member self.addSingleton(f: _ -> 't) = self.AddSingleton<'t> f
        member self.addSingletonValue(v: 'v) = self.AddSingleton<'v> v
        member self.addSingletonType(t: Type) = self.AddSingleton t

        member self.configureLogging(lr: LoggerRegister) =
            self.AddLogging(fun builder -> lr.configure builder)

open ext_IServiceCollection

type Builder with

    member self.build() =
        fun (sc: IServiceCollection) ->
            sc
                //日志注册器
                .addSingletonValue(
                    { LoggerProviders = []
                      LoggerFilters = [] }
                )
                //插件注册器
                .addSingletonValue(
                    { BeforeBuild = []; AfterBuild = [] }
                )
                //ID生成器
                .addSingleton(fun _ -> IPalaflakeGenerator.make 01uy)
                .addSingleton(fun _ -> IUuidGenerator.make ())
                //文章管道构造器
                .addSingleton(fun _ -> IPostInitPipelineBuilder.make ())
                .addSingleton(fun _ -> IPostRenderPipelineBuilder.make ())
                .addSingleton(fun _ -> IPostModifyPipelineBuilder.make ())
                .addSingleton(fun _ -> IPostFinalizePipelineBuilder.make ())
                //文章管道
                .addTransient(fun sf -> IPostInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> IPostRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> IPostModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf ->
                    IPostFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射文章提供器
                .addTransient(fun sf ->
                    IMappedPostProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //评论管道构造器
                .addSingleton(fun _ -> ICommentInitPipelineBuilder.make ())
                .addSingleton(fun _ -> ICommentRenderPipelineBuilder.make ())
                .addSingleton(fun _ -> ICommentModifyPipelineBuilder.make ())
                .addSingleton(fun _ -> ICommentFinalizePipelineBuilder.make ())
                //评论管道
                .addTransient(fun sf -> ICommentInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> ICommentRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> ICommentModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf ->
                    ICommentFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射评论提供器
                .addTransient(fun sf ->
                    IMappedCommentProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //用户管道构造器
                .addSingleton(fun _ -> IUserInitPipelineBuilder.make ())
                .addSingleton(fun _ -> IUserRenderPipelineBuilder.make ())
                .addSingleton(fun _ -> IUserModifyPipelineBuilder.make ())
                .addSingleton(fun _ -> IUserFinalizePipelineBuilder.make ())
                //用户管道
                .addTransient(fun sf ->
                    IUserInitPipeline.make (sf.GetRequiredService(), sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> IUserRenderPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf -> IUserModifyPipeline.make (sf.GetRequiredService(), sf.GetRequiredService()))
                .addTransient(fun sf ->
                    IUserFinalizePipeline.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
                //映射用户提供器
                .addTransient (fun sf ->
                    IMappedUserProvider.make (
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService(),
                        sf.GetRequiredService()
                    ))
        .> self.pipeline //use...
        .> fun sc -> //添加已注册日志
            sc.configureLogging (sc.BuildServiceProvider().GetRequiredService(): LoggerRegister)
        //before build
        .> fun sc ->
            sc
                .addSingleton(fun sf ->
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
                    ): IApp)
                .BuildServiceProvider()
        .> fun sp ->
            sp //启动BeforeBuild生命周期插件
                .GetRequiredService<PluginRegister>()
                .BeforeBuild
                .foldr
            <| fun pluginType (acc: IServiceProvider) ->
                //在新的容器中限定插件资产以及标识作用域
                ServiceCollection()
                    .addSingleton(fun _ -> sp.GetRequiredService(): IDbOperationBuilder)
                    .addSingleton(fun _ -> IPluginCfgProvider.make pluginType)
                    //文章构造资产
                    .addSingleton(fun _ -> sp.GetRequiredService(): IPostInitPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IPostRenderPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IPostModifyPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IPostFinalizePipelineBuilder)
                    .addTransient(fun _ -> sp.GetRequiredService(): IPostInitPipeline)
                    .addTransient(fun _ -> sp.GetRequiredService(): IPostRenderPipeline)
                    .addTransient(fun _ -> sp.GetRequiredService(): IPostModifyPipeline)
                    .addTransient(fun _ -> sp.GetRequiredService(): IPostFinalizePipeline)
                    .addTransient(fun _ -> sp.GetRequiredService(): IMappedPostProvider)
                    //评论构造资产
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentInitPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentRenderPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentModifyPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentFinalizePipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentInitPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentRenderPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentModifyPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): ICommentFinalizePipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IMappedCommentProvider)
                    //用户构造资产
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserInitPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserRenderPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserModifyPipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserFinalizePipelineBuilder)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserInitPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserRenderPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserModifyPipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IUserFinalizePipeline)
                    .addSingleton(fun _ -> sp.GetRequiredService(): IMappedUserProvider)
                    //...
                    .configureLogging(
                        sp.GetRequiredService(): LoggerRegister
                    )
                    //...
                    .addSingletonType(
                        pluginType
                    )
                    .BuildServiceProvider()
                    .GetRequiredService pluginType
                |> effect (fun _ -> //记录到日志
                    sp
                        .GetRequiredService<ILogger<Builder>>()
                        .LogInformation $"Plugin loaded: {pluginType.Name}")
                |> always acc
            <| sp
        .> fun sp ->
            sp.GetRequiredService<IApp>().effect //build
            <| fun app ->
                sp //启动AfterBuild生命周期插件
                    .GetRequiredService<PluginRegister>()
                    .AfterBuild
                    .foldr
                <| fun pluginType (acc: IServiceProvider) ->
                    //在新的容器中限定插件资产以及标识作用域
                    ServiceCollection()
                        .addSingleton(fun _ -> app) //IAPP
                        .addSingleton(fun _ -> sp.GetRequiredService(): IDbOperationBuilder)
                        .addSingleton(fun _ -> IPluginCfgProvider.make pluginType)
                        //文章管道资产
                        .addTransient(fun _ -> sp.GetRequiredService(): IPostInitPipeline)
                        .addTransient(fun _ -> sp.GetRequiredService(): IPostRenderPipeline)
                        .addTransient(fun _ -> sp.GetRequiredService(): IPostModifyPipeline)
                        .addTransient(fun _ -> sp.GetRequiredService(): IPostFinalizePipeline)
                        .addTransient(fun _ -> sp.GetRequiredService(): IMappedPostProvider)
                        //评论管道资产
                        .addSingleton(fun _ -> sp.GetRequiredService(): ICommentInitPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): ICommentRenderPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): ICommentModifyPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): ICommentFinalizePipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): IMappedCommentProvider)
                        //用户管道资产
                        .addSingleton(fun _ -> sp.GetRequiredService(): IUserInitPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): IUserRenderPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): IUserModifyPipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): IUserFinalizePipeline)
                        .addSingleton(fun _ -> sp.GetRequiredService(): IMappedUserProvider)
                        //...
                        .configureLogging(
                            sp.GetRequiredService(): LoggerRegister
                        )
                        //...
                        .addSingletonType(
                            pluginType
                        )
                        .BuildServiceProvider()
                        .GetRequiredService(pluginType)
                    |> effect (fun _ -> //记录到日志
                        sp
                            .GetRequiredService<ILogger<Builder>>()
                            .LogInformation $"Plugin loaded: {pluginType.Name}")
                    |> always acc
                <| sp
        |> apply (ServiceCollection())
