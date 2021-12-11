module pilipala.util.io

open System.IO
open System.Text

let readFile path = File.ReadAllText(path, Encoding.UTF8)

let writeFile (path: string) (text: string) = text |> (new StreamWriter(path)).Write
