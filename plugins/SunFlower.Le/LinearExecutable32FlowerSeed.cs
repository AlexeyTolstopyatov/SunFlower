using SunFlower.Abstractions;

namespace SunFlower.Le;

[FlowerSeedContract(2, 1, 0)]
public class LinearExecutable32FlowerSeed : IFlowerSeed
{
    public string Seed => "Sunflower OS/2-ArcaOS LX Any-CPU";
    public FlowerSeedStatus Status { get; } = new();
    public int Main(string path)
    {
        
        return 0;
    }
}