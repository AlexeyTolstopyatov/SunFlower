using System.Data;

namespace SunFlower.Abstractions;

/// <summary>
/// Main interface of all next following external DLLs
/// You must implement it if you want your plugins will work.
/// </summary>
public interface IFlowerSeed
{
    /// <summary>
    /// .NET external DLL compatible with SunFlower interface
    /// </summary>
    string Seed { get; }
    /// <summary>
    /// Expected result from image diagnostics
    /// </summary>
    FlowerSeedStatus Status { get; set; }

    /// <summary>
    /// EntryPoint of SunFlower Plugin
    /// Must return the status <see cref="DataTable"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    int Main(string path);
}