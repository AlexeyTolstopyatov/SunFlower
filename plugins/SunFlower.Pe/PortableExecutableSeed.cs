using SunFlower.Abstractions;
using SunFlower.Abstractions.Attributes;

namespace SunFlower.Pe;

/// <summary>
/// Template of IFlowerSeed implementation.
/// </summary>
[FlowerSeedContract(Version = "1.2")] // <-- important flags
public class PortableExecutableSeed : IFlowerSeed
{
    public string Seed => "Sunflower seed Windows PE IA-32(e)";
    public FlowerSeedStatus Status { get; set; } = new FlowerSeedStatus();
    
    /// <summary>
    /// EntryPoint returns Status Table 
    /// </summary>
    /// <returns></returns>
    public int Main(string path) // <-- path to needed image 
    {
        try
        {
            return 0;
        }
        catch
        {
            return -1;
        }
    }
}