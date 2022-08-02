namespace pilipala.plugin

open pilipala.util.io

module IPluginCfgProvider =

    let make<'t> () =

        let configPath =
            $"./plugin/{typeof<'t>.Name}/config.json"

        { new IPluginCfgProvider with
            member i.config
                with get () = readFile configPath
                and set v = writeFile configPath v }
