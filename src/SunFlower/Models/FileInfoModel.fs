namespace SunFlower.Models

open System
open SunFlower.Kernel.Readers

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
