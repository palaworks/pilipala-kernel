namespace pilipala.builder

open Microsoft.Extensions.DependencyInjection
open fsharper.typ
open pilipala
open pilipala.container.comment
open pilipala.container.post
open pilipala.data.db
open pilipala.id
open pilipala.log
open pilipala.service
open pilipala.plugin
open pilipala.pipeline.post
open pilipala.pipeline.comment

type Builder =
    { pipeline: IServiceCollection -> IServiceCollection }

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
        fun (sc: IServiceCollection) ->
            sc
                .AddSingleton<LogRegister>()
                .AddSingleton<ServiceRegister>()
                //ID生成器
                .AddSingleton<IPalaflakeGenerator>(fun _ -> IPalaflakeGenerator.make 01uy)
                .AddSingleton<IUuidGenerator>(fun _ -> IUuidGenerator.make ())
                //文章管道构造器
                .AddSingleton<IPostInitPipelineBuilder>(fun _ -> IPostInitPipelineBuilder.make ())
                .AddSingleton<IPostRenderPipelineBuilder>(fun _ -> IPostRenderPipelineBuilder.make ())
                .AddSingleton<IPostModifyPipelineBuilder>(fun _ -> IPostModifyPipelineBuilder.make ())
                .AddSingleton<IPostFinalizePipelineBuilder>(fun _ -> IPostFinalizePipelineBuilder.make ())
                //文章管道
                .AddTransient<IPostInitPipeline>(fun sf ->
                    IPostInitPipeline.make (
                        sf.GetService<IPostInitPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<IPostRenderPipeline>(fun sf ->
                    IPostRenderPipeline.make (
                        sf.GetService<IPostRenderPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<IPostModifyPipeline>(fun sf ->
                    IPostModifyPipeline.make (
                        sf.GetService<IPostModifyPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<IPostFinalizePipeline>(fun sf ->
                    IPostFinalizePipeline.make (
                        sf.GetService<IPostRenderPipelineBuilder>(),
                        sf.GetService<IPostFinalizePipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                //映射文章提供器
                .AddTransient<IMappedPostProvider>(fun sf ->
                    IMappedPostProvider.make (
                        sf.GetService<IPostInitPipeline>(),
                        sf.GetService<IPostRenderPipeline>(),
                        sf.GetService<IPostModifyPipeline>(),
                        sf.GetService<IPostFinalizePipeline>()
                    ))
                //评论管道构造器
                .AddSingleton<ICommentInitPipelineBuilder>(fun _ -> ICommentInitPipelineBuilder.make ())
                .AddSingleton<ICommentRenderPipelineBuilder>(fun _ -> ICommentRenderPipelineBuilder.make ())
                .AddSingleton<ICommentModifyPipelineBuilder>(fun _ -> ICommentModifyPipelineBuilder.make ())
                .AddSingleton<ICommentFinalizePipelineBuilder>(fun _ -> ICommentFinalizePipelineBuilder.make ())
                //评论管道
                .AddTransient<ICommentInitPipeline>(fun sf ->
                    ICommentInitPipeline.make (
                        sf.GetService<ICommentInitPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<ICommentRenderPipeline>(fun sf ->
                    ICommentRenderPipeline.make (
                        sf.GetService<ICommentRenderPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<ICommentModifyPipeline>(fun sf ->
                    ICommentModifyPipeline.make (
                        sf.GetService<ICommentModifyPipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                .AddTransient<ICommentFinalizePipeline>(fun sf ->
                    ICommentFinalizePipeline.make (
                        sf.GetService<ICommentRenderPipelineBuilder>(),
                        sf.GetService<ICommentFinalizePipelineBuilder>(),
                        sf.GetService<IDbOperationBuilder>()
                    ))
                //映射评论提供器
                .AddTransient<IMappedCommentProvider>(fun sf ->
                    IMappedCommentProvider.make (
                        sf.GetService<ICommentInitPipeline>(),
                        sf.GetService<ICommentRenderPipeline>(),
                        sf.GetService<ICommentModifyPipeline>(),
                        sf.GetService<ICommentFinalizePipeline>()
                    ))
        .> self.pipeline
        .> fun sc -> sc.AddSingleton<Pilipala>()
        |> apply (ServiceCollection())
        |> fun sc -> sc.BuildServiceProvider().GetService<Pilipala>()
