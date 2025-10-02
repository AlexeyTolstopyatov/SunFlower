using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Le.Services;

namespace SunFlower.Le;

[FlowerSeedContract(3, 0, 0)]
public class LinearExecutable32FlowerSeed : IFlowerSeed
{
    public string Seed => "Sunflower OS/2-ArcaOS LX Any-CPU";
    public FlowerSeedStatus Status { get; } = new();
    public int Main(string path)
    {
        try
        {
            LxDumpManager dumpManager = new(path);
            LxTableManager tableManager = new(dumpManager);

            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Strings, tableManager.Characteristics));
            FlowerSeedResult imports = new(FlowerSeedEntryType.Strings);
            
            List<string> mods = ["### Imported Modules", ..tableManager.ImportedNames];
            List<string> procs = ["### Imported Procedures", ..tableManager.ImportedProcedures];
            
            mods.AddRange(procs);

            imports.BoxedResult = mods;
            Status.Results.Add(imports);
            
            List<DataTable> unboxed = [..tableManager.Headers];
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.DataTables, unboxed));
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions, tableManager.ObjectRegions));
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions, tableManager.EntryTableRegions));
            Status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions, tableManager.NamesRegions));
            
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