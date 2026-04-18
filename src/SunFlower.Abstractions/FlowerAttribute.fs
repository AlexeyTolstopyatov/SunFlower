namespace SunFlower.Abstractions

open System
//
// CoffeeLake (C) 2026
// This part licensed under MIT
//
type SeedTarget =
    /// This characteristic means the plugin specified
    /// on the program data (embedded structures or nested tables)
    ///
    /// For example: runtime walker or packer
    | Data = 0
    /// This characteristic means the plugin specifies
    /// on the text/code analysis
    ///
    /// For example: disassembler for target CPU architecture
    | Code = 1
    /// This characteristic means that plugin runs target
    /// by itself and works with process, instead of file
    | Process = 2

/// <summary>
/// This attribute is the main for sunflower plugins
/// Metadata which stores here could be used at the kernel or at the client-side
/// </summary>
[<Sealed>]
type FlowerAttribute(target: SeedTarget) =
    inherit Attribute()
    let mutable _target: SeedTarget = target
    member val Target: SeedTarget = _target with get, set
