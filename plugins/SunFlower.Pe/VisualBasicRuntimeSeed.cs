using SunFlower.Abstractions;
using SunFlower.Pe.VisualBasic;

namespace SunFlower.Pe;

[FlowerSeedContract(2, 0, 0)]
public class VisualBasicRuntimeSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Basic";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            
            return 0;
        }
        catch (Exception e)
        {
            
            Status.LastError = e;
            return -1;
        }
    }
}