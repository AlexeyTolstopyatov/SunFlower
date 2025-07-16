using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;
using FileStream = System.IO.FileStream;

namespace SunFlower.Pe.Services;

///
/// CoffeeLake 2024-2025
/// This code is JellyBins part for dumping
/// Windows PE32/+ images.
///
/// Licensed under MIT
///

public class PeDumpManager(string path) : UnsafeManager, IManager
{
    public static PeDumpManager CreateInstance(string path) => new(path);
    public MzHeader Dos2Header { get; set; }
    public PeFileHeader FileHeader { get; set; }
    public PeOptionalHeader32 OptionalHeader32 { get; set; }
    public PeOptionalHeader OptionalHeader { get; set; }
    public PeDirectory[] PeDirectories { get; set; } = [];
    public PeSection[] PeSections { get; set; } = [];
    public FileSectionsInfo FileSectionsInfo { get; set; } = new();
    public bool Is64Bit { get; set; }

    /// <summary>
    /// Starts manager in another thread
    /// </summary>
    public void Initialize()
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        FindHeaders(reader);
        FindSectionsTable(reader);
            
        reader.Close();
        
        FileSectionsInfo info = new()
        {
            FileAlignment = Is64Bit ? OptionalHeader.SectionAlignment : OptionalHeader32.SectionAlignment,
            SectionAlignment = Is64Bit ? OptionalHeader.SectionAlignment : OptionalHeader32.SectionAlignment,
            ImageBase = Is64Bit ? OptionalHeader.ImageBase : OptionalHeader32.ImageBase,
            BaseOfCode = Is64Bit ? OptionalHeader.BaseOfCode : OptionalHeader32.BaseOfCode,
            BaseOfData = Is64Bit ? OptionalHeader.BaseOfData : OptionalHeader32.BaseOfData,
            Sections = PeSections,
            Directories = PeDirectories,
            NumberOfSections = FileHeader.NumberOfSections,
            NumberOfRva = Is64Bit ? OptionalHeader.NumberOfRvaAndSizes : OptionalHeader32.NumberOfRvaAndSizes,
            Is64Bit = Is64Bit
        };
        
        FileSectionsInfo = info;
    }
    private void FindHeaders(BinaryReader reader)
    {
        var dos2 = reader.ReadUInt16();
        if (dos2 != 0x5A4D && dos2 != 0x4D5A)
        {
            throw new InvalidOperationException("Not a DOS/2 signature");
        }
    
        reader.BaseStream.Position = 0;
        var dos2Hdr = Fill<MzHeader>(reader);
        Dos2Header = dos2Hdr;

        reader.BaseStream.Position = dos2Hdr.e_lfanew;

        var peSignature = reader.ReadUInt32();
        if (peSignature != 0x00004550)
        {
            throw new InvalidOperationException("Not a PE signature");
        }

        var fileHdr = Fill<PeFileHeader>(reader);
        FileHeader = fileHdr;

        Is64Bit = fileHdr.Machine switch
        {
            0x8664 => true,
            0x0200 => true,
            _ => false 
        };

        if (Is64Bit)
        {
            OptionalHeader = Fill<PeOptionalHeader>(reader);
            PeDirectories = OptionalHeader.Directories;
        }
        else
        {
            OptionalHeader32 = Fill<PeOptionalHeader32>(reader);
            PeDirectories = OptionalHeader32.Directories;
        }
    }

    private void FindSectionsTable(BinaryReader reader)
    {
        List<PeSection> sections = [];
        for (UInt32 i = 0; i < FileHeader.NumberOfSections; ++i)
        {
            sections.Add(Fill<PeSection>(reader));
        }

        PeSections = sections.ToArray();
    }
}