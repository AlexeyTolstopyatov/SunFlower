using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Services;

namespace SunFlower.Ne;

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
            
            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.Text,
                BoxedResult = tableManager.Characteristics
            });
            List<DataTable> unboxed =
            [
                tableManager.Headers[0],
                tableManager.Headers[1],
                tableManager.SegmentTable,
                tableManager.EntryPointsTable,
                tableManager.ModuleReferencesTable,
                tableManager.NamesTable
            ];
            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.DataTables,
                BoxedResult = unboxed
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