using SunFlower.Abstractions;
using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;
using SunFlower.Pe.Services;

namespace SunFlower.Pe;

[FlowerSeedContract(3, 0, 0)]
public class VisualBasicFlowerSeed : IFlowerSeed
{
    public string Seed => "Sunflower VisualBasic Runtime";
    public FlowerSeedStatus Status { get; } = new();
    
    public int Main(string path)
    {
        try
        {
            var peManager = new PeDumpManager(path);
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

            if (isVb5Defined)
                FindVb5Details(peManager.VbOffset, path, peManager.Vb5Header, sectionsInfo);
        }
        catch (Exception e)
        {
            Status.LastError = e;
        }

        return -1;
    }

    private static FlowerSeedStatus FindVb5Details(long offset, string path, Vb5Header header, FileSectionsInfo info)
    {
        var status = new FlowerSeedStatus();

        var data = new Vb5ProjectTablesManager(path, offset, header, info);
        
        return status;
    }
}