namespace pilipala.plugin

open System
open pilipala.util.io

module IPluginCfgProvider =

    let make (t: Type) =

        let configPath =
            $"./config/plugin/{t.Name}/config.json"

        { new IPluginCfgProvider with
            member i.config
                with get () = readFile configPath
                and set v = writeFile configPath v }
