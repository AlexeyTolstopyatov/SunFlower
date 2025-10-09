using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Services;

namespace SunFlower.Ne;

[FlowerSeedContract(3,0,0)]
public class NewExecutableSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Win16-OS/2 NE IA-32";
    public FlowerSeedStatus Status { get; } = new();
    public int Main(string path)
    {
        try
        {
            NeDumpManager dumpManager = new(path);
            NeTableManager tableManager = new(dumpManager);
            Status.IsEnabled = true;

            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings, tableManager.Characteristics));
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions, tableManager.Regions));
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}