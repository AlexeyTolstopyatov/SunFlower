using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using Sunflower.Mz.Services;

namespace Sunflower.Mz;

[FlowerSeedContract(2, 0, 0)]
public class MarkZbikowskiFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower PC/MS-DOS MZ i8086+";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            MzDumpManager manager = new(path);
            MzTableManager tableManager = new(manager);
            
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.Regions
            });

            Status.IsEnabled = true;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            throw;
        }
        return 0;
    }
}