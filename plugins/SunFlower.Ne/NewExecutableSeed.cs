using SunFlower.Abstractions;

namespace SunFlower.Ne;

public class NewExecutableSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Win16-OS/2 NE IA32";
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