using System.Data;
using System.Text;
using Microsoft.VisualBasic;
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
            Status.Results.Add(new FlowerSeedResult()
            {
                Type = FlowerSeedEntryType.Text,
                BoxedResult = tableManager.Imports
            });
            List<DataTable> unboxed =
            [
                ..tableManager.Headers,
                tableManager.SegmentTable,
                ..tableManager.RelocationsTables,
                ..tableManager.EntryTables,
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
    /// <summary>
    /// Prints hexadecimal map of caught bytes slice
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    private string GetHexViewOf(string title, byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"### {title}");
        
        // 00 00 00 00 00 00 00 00 |........| 
        // 00 FF FF 30 65 66 67 31 |.яя0abc1|
        // ...
        
        
        
        return sb.ToString();
    }
}