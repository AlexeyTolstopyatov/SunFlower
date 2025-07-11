using SunFlower.Abstractions;
using SunFlower.Le.Services;

namespace SunFlower.Le;

public class LinearExecutableFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "LE Sunflower Win32s-OS/2 Any-CPU";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            LeDumpManager dumpManager = new(path);
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}