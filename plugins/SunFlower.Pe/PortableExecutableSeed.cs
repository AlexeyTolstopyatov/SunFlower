using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;
using SunFlower.Pe.Services;

namespace SunFlower.Pe;

/// <summary>
/// Template of IFlowerSeed implementation.
/// </summary>
[FlowerSeedContract(MajorVersion = 2, MinorVersion = 0, BuildVersion = 0)] // <-- important flags
public class PortableExecutableSeed : IFlowerSeed
{
    public string Seed => "Sunflower Windows PE32/+ IA-32(e)";
    public FlowerSeedStatus Status { get; set; } = new FlowerSeedStatus();
    
    /// <summary>
    /// EntryPoint returns Status Table 
    /// </summary>
    /// <returns></returns>
    public int Main(string path) // <-- path to needed image 
    {
        try
        {
            PeDumpManager dumpManager = new(path);
            dumpManager.Initialize();
            
            PeExportsManager exportsManager = new(dumpManager.FileSectionsInfo, path);
            PeImportsManager importsManager = new(dumpManager.FileSectionsInfo, path);
            PeClrManager clrManager = new(dumpManager.FileSectionsInfo, path);
            
            exportsManager.Initialize();
            importsManager.Initialize();
            clrManager.Initialize();

            PeImageModel image = new()
            {
                Sections = dumpManager.PeSections,
                OptionalHeader = dumpManager.OptionalHeader,
                OptionalHeader32 = dumpManager.OptionalHeader32,
                FileHeader = dumpManager.FileHeader,
                MzHeader = dumpManager.Dos2Header,
                ExportTableModel = exportsManager.ExportTableModel,
                ImportTableModel = importsManager.ImportTableModel,
                CorHeader = clrManager.Cor20Header
            };

            PeTableManager manager = new(image);
            manager.Initialize();
            
            FlowerSeedResult start = new(FlowerSeedEntryType.Strings)
            {
                BoxedResult = new string[]
                {
                    "### Image structure details", 
                    $"Target: `{new FileInfo(path).Name}`", 
                    $"Target size: `{new FileInfo(path).Length / 1024}K`"
                }
            };
            FlowerSeedResult tables = new(FlowerSeedEntryType.DataTables)
            {
                BoxedResult = manager.Results
            };
            Status.Results.Add(start);
            Status.Results.Add(tables);
            
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