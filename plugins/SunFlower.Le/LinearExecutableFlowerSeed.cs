using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Le.Services;

namespace SunFlower.Le;

[FlowerSeedContract(2, 1, 0)]
public class LinearExecutableFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Windows-OS/2 LE Any-CPU";
    public FlowerSeedStatus Status { get; private set; } = new();

    public int Main(string path)
    {
        try
        {
            LeDumpManager dumpManager = new(path);
            LeTableManager tableManager = new(dumpManager);

            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings)
            {
                BoxedResult = tableManager.Characteristics
            });
            FlowerSeedResult imports = new(FlowerSeedEntryType.Strings);
            List<string> mods = ["### Imported Modules", ..tableManager.ImportedNames];
            List<string> procs = ["### Imported Procedures", ..tableManager.ImportedProcedures];
            
            mods.AddRange(procs);

            imports.BoxedResult = mods;
            Status.Results.Add(imports);
            
            List<DataTable> unboxed = [..tableManager.Headers,];
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.DataTables)
            {
                BoxedResult = unboxed
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.ObjectRegions
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.EntryTableRegions
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.NamesRegions
            });
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions)
            {
                BoxedResult = tableManager.DriverRegions
            });
            Status.IsEnabled = true;
            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
            return -1;
        }
    }
}