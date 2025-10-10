namespace SunFlower.Abstractions

open System
open System.Text
open Microsoft.FSharp.Core
[<Class>]
type FlowerDescriptor() =
    member private i.container : StringBuilder = StringBuilder()
    
    [<CompiledName "Line">]
    member public i.Line (text : string) : FlowerDescriptor =
        if not (String.IsNullOrEmpty(text)) then
            i.container.AppendLine(text) |> ignore
        i
        
    [<CompiledName "Inline">]
    member public i.Inline (text : string) : FlowerDescriptor =
        if not (String.IsNullOrEmpty(text)) then
            i.container.Append(text) |> ignore
        i
        
    [<CompiledName "Clear">]
    member public i.Clear() : FlowerDescriptor =
        i.container.Clear() |> ignore
        i
        
    override i.ToString() = 
        if i.container.Length > 0 then 
            i.container.ToString() 
        else 
            "No description available"