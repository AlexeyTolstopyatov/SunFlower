namespace SunFlower.Abstractions
open System

///
/// CoffeeLake 2025
/// this code licensed under (see GitHub repo) license
///
/// @creator: atolstopyatov2017@vk.com
///

/// <summary>
/// Version contract between plugin-kernel sides
/// </summary>
[<Class>]
[<AttributeUsage(AttributeTargets.Class)>]
type FlowerSeedContractAttribute(major: int, minor: int, build: int) =
    inherit Attribute() with
    
    let mutable major_version : Int32 = major
    let mutable minor_version : Int32 = minor
    let mutable build_version : Int32 = build
    
    new() =
        FlowerSeedContractAttribute(1, 3, 0)
    /// <summary>
    /// See Microsoft versioning specification
    /// Major means global changes in software
    ///
    /// 1.3.1 - FlowerContract looks like ...
    /// 2.0.0 - Global changes in FlowerContract. Fully incompatible with 1.x.x
    /// </summary>
    member val MajorVersion = major_version with get, set
    /// <summary>
    /// See Microsoft versioning specification
    /// Minor means inner-scope changes without corrupting/changing public API
    ///
    /// 1.3.1 - FlowerContract
    /// 1.4.0 - Compatible changes in FlowerContract but backwards usable
    /// </summary>
    member val MinorVersion = minor_version with get, set
    /// <summary>
    /// My specification tells:
    /// Build version means not #build and little but important changes
    /// in target build. By this magic number you can track issues
    ///
    /// 1.3.1 - has internal panic and MainWindow crash
    /// 1.3.2 - fix of .1 internal panic
    /// </summary>
    member val BuildVersion = build_version with get, set
    