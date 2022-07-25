namespace pilipala.plugin

open pilipala.util.io

module IPluginCfgProvider =
    let make<'t> () =
        { new IPluginCfgProvider with
            member i.config =
                readFile $"./plugin/{typeof<'t>.Name}/config.json" }
