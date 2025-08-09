using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Services;

namespace SunFlower.Le;

public class LinearExecutableFlowerSeed : IFlowerSeed
{
    public string Seed { get; } = "Sunflower Win32s-OS/2 LE Any-CPU";
    public FlowerSeedStatus Status { get; set; } = new();

    public int Main(string path)
    {
        try
        {
            LeDumpManager dumpManager = new(path);
            LeTableManager tableManager = new(dumpManager);

            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.Strings,
                BoxedResult = tableManager.Characteristics
            });
            FlowerSeedResult imports = new()
            {
                Type = FlowerSeedEntryType.Strings
            };
            List<string> mods = ["### Imported Modules", ..tableManager.ImportedNames];
            List<string> procs = ["### Imported Procedures", ..tableManager.ImportedProcedures];
            
            mods.AddRange(procs);

            imports.BoxedResult = mods;
            Status.Results.Add(imports);
            
            List<DataTable> unboxed =
            [
                ..tableManager.Headers,
                tableManager.ObjectsTable,
                tableManager.ObjectPages,
                tableManager.FixupPages,
                //..tableManager.FixupRecords,
            ];
            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.DataTables,
                BoxedResult = unboxed
            });
            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.Regions,
                BoxedResult = tableManager.EntryTableRegions
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