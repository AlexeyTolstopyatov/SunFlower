using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Pe.Models;
using SunFlower.Pe.Services;

namespace SunFlower.Pe;

[FlowerSeedContract(3, 0, 0)]
public class VisualBasicFlowerSeed : IFlowerSeed
{
    public string Seed => "Sunflower VisualBasic Runtime";
    public FlowerSeedStatus Status { get; private set; } = new();
    
    public int Main(string path)
    {
        try
        {
            var peManager = new PeDumpManager(path);
            peManager.Initialize();
            
            var sectionsInfo = new FileSectionsInfo
            {
                BaseOfCode = peManager.OptionalHeader32.BaseOfCode,
                ImageBase = peManager.OptionalHeader32.ImageBase,
                BaseOfData = peManager.OptionalHeader32.BaseOfData,
                EntryPoint = peManager.OptionalHeader32.AddressOfEntryPoint,
                FileAlignment = peManager.OptionalHeader32.FileAlignment,
                Is64Bit = false,
                NumberOfRva = peManager.OptionalHeader32.NumberOfRvaAndSizes,
                NumberOfSections = peManager.FileHeader.NumberOfSections,
                Sections = peManager.PeSections,
                Directories = peManager.PeDirectories,
                SectionAlignment = peManager.OptionalHeader32.SectionAlignment
            };
            // null! tells that struct by-value initialized already
            // but the Magic signature not. Magic.Length will throw NullReferenceException.

            var isVb5Defined = peManager.Vb5Header.VbMagic != null!;
            var isVb4Defined = peManager.Vb4Header.Signature != null!;

            var status = new FlowerSeedStatus();
            
            if (isVb5Defined)
                status = FindVb5Details(peManager.VbOffset, path, peManager, sectionsInfo);

            Status = status;
            Status.IsEnabled = true;            
            return 0;
        }
        catch (Exception e)
        {
            Status.LastError = e;
        }

        return -1;
    }

    private static FlowerSeedStatus FindVb5Details(long offset, string path, PeDumpManager manager, FileSectionsInfo info)
    {
        var status = new FlowerSeedStatus();
        
        var data = new Vb5ProjectTablesManager(path, offset, manager.Vb5Header, info);

        status.Results.Add(new FlowerSeedResult(FlowerSeedEntryType.Regions, new VbImageView(manager.Vb5Header, data).Regions));
        
        return status;
    }
}