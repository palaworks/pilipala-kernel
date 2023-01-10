namespace pilipala.plugin

open System

(*
插件需要遵循下列规范：
插件根目录：
./pilipala/plugin
插件文件夹名和插件dll名和插件名需一致：
./pilipala/plugin/Llink/Llink.dll
插件的启动应由其构造函数完成，这与服务所谓的入口点不同。
*)

//虽然理论上该实现能够启动同一文件夹下的众多插件（dll），但建议的实践是一个插件（dll）放一个文件夹

//dir目录下应有多个文件夹
//每个文件夹对应一个插件，例如：
//./pilipala/plugin/Llink
//./pilipala/plugin/Palang
//./pilipala/plugin/Mailssage

type PluginRegister =
    { BeforeBuild: Type list
      AfterBuild: Type list }

//最终整合时应使用foldr以保证顺序
type PluginRegister with
    member self.registerPlugin t hookTime =
        if hookTime = AppLifeCycle.BeforeBuild then
            { self with BeforeBuild = t :: self.BeforeBuild }
        else
            { self with AfterBuild = t :: self.AfterBuild }
