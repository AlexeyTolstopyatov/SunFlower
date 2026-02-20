namespace SunFlower.Models

open System
open SunFlower.Kernel.Readers
// CoffeeLake (C) 2026-*
// MIT
//
// This code contains abstraction under the Sunflower kernel
// Middleware fully copies the by-value SunFlower.Kernel.FlowerFileInfo
// datatype. Uses by high-level (client) entities 
[<Class>]
type FileInfoModel(seeker: FlowerFileInfo) =
    /// <summary>
    /// Name of target
    /// </summary>
    member this.Name: string = seeker.Name
    /// <summary>
    /// Filesystem path of target
    /// </summary>
    member this.Path: string = seeker.Path
    /// <summary>
    /// Size of target
    /// </summary>
    member this.Size: Single = seeker.Size
    /// <summary>
    /// Hexadecimal signature view
    /// </summary>
    member this.Sign: string = seeker.Sign
    /// <summary>
    /// Description of signature
    /// </summary>
    member this.Type : string = seeker.Type
