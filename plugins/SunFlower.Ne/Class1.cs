using SunFlower.Abstractions;

namespace SunFlower.Ne;

public class NewExecutableSeed : IFlowerSeed
{
    public string Seed { get; }
    public FlowerSeedStatus Status { get; set; }
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