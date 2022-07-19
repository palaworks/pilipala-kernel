module pilipala.util.io

open System.IO
open System.Text

let inline readFile path = File.ReadAllText(path, Encoding.UTF8)

let inline writeFile (path: string) (text: string) = text |> (new StreamWriter(path)).Write
