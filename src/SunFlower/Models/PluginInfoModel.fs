namespace SunFlower.Models
// CoffeeLake (C) 2026-*
// MIT
//
// This class is a shorten metadata about all activated
// interfaces instances of external connected .NET DLLs (Sunflower Seeds)
//
// More information contains SunFlower.Terminal CLI application
// SunFlower client has an ability to call it in new session
// if more information required
open SunFlower.Abstractions

[<Class>]
type PluginInfoModel() =
        
    member this.Instance: string = "?"
    member this.Contract: string = "?"
    
    // static member Collect(): PluginInfoModel
    //     SunFlower.Terminal.Program
    //


