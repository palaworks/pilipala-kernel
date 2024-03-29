[<AutoOpen>]
module pilipala.builder.useDb

open Microsoft.Extensions.DependencyInjection
open fsharper.typ
open pilipala.builder
open pilipala.data.db

type Builder with

    /// 启用持久化队列
    /// 启用该选项会延迟数据持久化以缓解数据库压力并提升访问速度
    member self.useDb(config: DbConfig) =

        let f (sc: IServiceCollection) =
            //全局数据库单例
            sc.AddSingleton<IDbOperationBuilder>(fun _ -> IDbOperationBuilder.make config)

        { pipeline = self.pipeline .> f }
