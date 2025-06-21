using System.Data;

namespace SunFlower.Abstractions;

public interface IFlowerSeed
{
    /// <summary>
    /// .NET external DLL compatible with SunFlower interface
    /// </summary>
    string Seed { get; }
    /// <summary>
    /// Expected result from image diagnostics
    /// </summary>
    FlowerSeedResult Result { get; set; }
    /// <summary>
    /// EntryPoint of SunFlower Plugin
    /// Must return the status <see cref="DataTable"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    int Main(string path);
    /// <summary>
    /// Finds result of analysing target binary
    /// </summary>
    /// <param name="path"></param>
    void Analyse(string path);
}