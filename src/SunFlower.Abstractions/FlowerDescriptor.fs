namespace SunFlower.Abstractions

open System.Text
open Microsoft.FSharp.Core

[<Class>]
type FlowerDescriptor() =
    member private i.container : StringBuilder = StringBuilder() // already initialized
    
    [<CompiledName "Line">]
    member public i.line (text : string) : FlowerDescriptor =
        i.container.AppendLine(text) |> ignore
        i
        
    [<CompiledName "Inline">]
    member public i.in_line (text : string) : FlowerDescriptor =
        i.container.Append($" `{text}` ") |> ignore
        i
        
    override i.ToString() = i.container.ToString()