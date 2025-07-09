using SunFlower.Abstractions;

namespace SunFlower.Le;

public class LinearExecutableFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "LE Win32s-OS/2 Any-CPU";
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