using System.Data;
using System.Diagnostics;
using SunFlower.Abstractions;

namespace SunFlower.Pe;

/// <summary>
/// Template of IFlowerSeed implementation.
/// </summary>
public class PortableExecutableSeed : IFlowerSeed
{
    public string Seed => "Sunflower seed Windows PE IA-32(e)";
    public FlowerSeedStatus Status { get; set; } = new FlowerSeedStatus();

    /// <summary>
    /// Second important functional
    /// Finds and fills <see cref="Status"/>
    /// </summary>
    /// <param name="path"></param>
    public void Analyse(string path)
    {
        try
        {
            
        }
        catch (Exception ex)
        {
            Debug.Write($"Plugin: {ex}");
            Status.IsEnabled = false;
        }
    }
    
    /// <summary>
    /// EntryPoint returns Status Table 
    /// </summary>
    /// <returns></returns>
    public int Main(string path) // <-- path to needed image 
    {
        try
        {
            Analyse(path);
            return Status.IsEnabled ? 0 : 1;
        }
        catch
        {
            return -1;
        }
    }
}