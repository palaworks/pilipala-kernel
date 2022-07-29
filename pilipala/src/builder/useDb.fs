[<AutoOpen>]
module pilipala.builder.useDb

open fsharper.typ.Pipe
open Microsoft.Extensions.DependencyInjection
open pilipala.builder
open pilipala.data.db

type Builder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDb(config: DbConfig) =

        let f (sc: IServiceCollection) =
            sc.AddSingleton<IDbOperationBuilder>(fun _ -> IDbOperationBuilder.make config)

        { pipeline = self.pipeline.export (StatePipe(activate = f)) }
