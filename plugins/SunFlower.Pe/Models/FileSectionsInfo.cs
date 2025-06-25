using SunFlower.Pe.Headers;
using SunFlower.Pe.Services;

namespace SunFlower.Pe.Models;

/// <summary>
/// Important details about PE32/+
/// for <see cref="PeExportsManager"/>
/// </summary>
public class FileSectionsInfo
{
    public bool Is64Bit { get; set; }
    public UInt32 NumberOfSections { get; set; }
    public UInt32 NumberOfRva { get; set; }
    
    public UInt32 SectionAlignment { get; set; }
    public UInt32 FileAlignment { get; set; }
    
    public UInt32 ImageBase { get; set; }
    public UInt32 BaseOfCode { get; set; }
    public UInt32 BaseOfData { get; set; }
    
    public PeSection[] Sections { get; set; } = [];
    public PeDirectory[] Directories { get; set; } = [];
}