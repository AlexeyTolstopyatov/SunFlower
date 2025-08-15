namespace SunFlower.Abstractions

open System
/// <summary>
/// EntryPoint of SunFlower Plugin
/// Must return the status <see cref="DataTable"/>
/// </summary>
/// <param name="path"></param>
/// <returns></returns>
type IFlowerSeed = interface
    /// <summary>
    /// .NET external DLL compatible with SunFlower interface
    /// </summary>
    abstract member Seed : String
        with get
    /// <summary>
    /// Expected result from image diagnostics
    /// </summary>
    abstract member Status : FlowerSeedStatus
        with get
    /// <summary>
    /// EntryPoint of SunFlower Plugin
    /// Must return the status <see cref="DataTable"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    abstract member Main : path : String -> Int32
    
    end

