using System.Data;
using System.Text;
using Microsoft.VisualBasic;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Services;

namespace SunFlower.Ne;

[FlowerSeedContract(2,0,0)]
public class NewExecutableSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Win16-OS/2 NE IA-32";
    public FlowerSeedStatus Status { get; set; } = new();
    public int Main(string path)
    {
        try
        {
            NeDumpManager dumpManager = new(path);
            NeTableManager tableManager = new(dumpManager);
            Status.IsEnabled = true;
            
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = tableManager.Characteristics
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.DataTables)
            {
                BoxedResult = tableManager.Headers
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = tableManager.Imports
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.ModulesRegion
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.SegmentRegions
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.EntryBundlesRegions
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.NamesRegions
            });
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}