using System.Diagnostics;
using Sunflower.Links.Headers;

namespace SunFlower.Links.Services;

public class PifDumpManager : UnsafeManager
{
    /// <summary>
    /// Main and necessary section in PIF binary
    /// </summary>
    public MicrosoftPifEx MicrosoftPifEx { get; set; } = new();

    /// <summary>
    /// Windows 3x Standard Mode section
    /// </summary>
    public Windows3x286 Windows3X286 { get; set; } = new();

    /// <summary>
    /// Windows 3x Extended Mode section
    /// </summary>
    public Windows3x386 Windows3X386 { get; set; } = new();

    /// <summary>
    /// Windows 9x Virtual machine Manager section
    /// </summary>
    public Windows4xVmm Windows4XVmm { get; set; } = new();

    /// <summary>
    /// Windows NT 3.1 Section /used by NT-VDM, I suggest/
    /// </summary>
    public WindowsNt3 WindowsNt3 { get; set; } = new();

    public List<PifSectionHead> SectionHeads { get; set; } = [];

    public PifDumpManager(string path)
    {
        Initialize(path);
    }

    private void Initialize(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        
        var length = reader.ReadUInt16();
        long baseSectionPosition = 0;
        
        if (length == 0x171) // Old Format (Windows 1.X/2.X)
        {
            stream.Position = 0;
            MicrosoftPifEx = Fill<MicrosoftPifEx>(reader);
            baseSectionPosition = 0x171;
        }
        else if (length > 0x171) // New Format
        {
            // Section
            stream.Position = 0x0;
            MicrosoftPifEx = Fill<MicrosoftPifEx>(reader);

            // Section Header
            stream.Position = 0x171;
            SectionHeads.Add(Fill<PifSectionHead>(reader));
            baseSectionPosition = 0x187;
        }
        else
        {
            throw new InvalidDataException("Unable to read file like Program Information File (PIF)");
        }

        var nextSectionOffset = baseSectionPosition;
        var safetyCounter = 0;
        const int maxSections = 10; // avoid infinite loop
        
        while (nextSectionOffset > 0 && 
               nextSectionOffset < stream.Length && 
               safetyCounter++ < maxSections)
        {
            stream.Position = nextSectionOffset;
            
            var sectionHead = Fill<PifSectionHead>(reader);
            SectionHeads.Add(sectionHead);
            
            if (sectionHead.NextSectionOffset == 0xFFFF)
            {
                // 0xFFFF offset means last section in file. Force exit.
                stream.Position = sectionHead.PartitionOffset;
                ReadSectionData(reader, sectionHead);
                break;
            }
            if (sectionHead.NextSectionOffset < stream.Length)
            {
                stream.Position = sectionHead.PartitionOffset;
                
                ReadSectionData(reader, sectionHead);
                
                stream.Position = sectionHead.NextSectionOffset;
            }
            else
            {
                Debug.WriteLine("Invalid data offset, skipping section");
            }
            
            nextSectionOffset = sectionHead.NextSectionOffset;
        }
    }

    private void ReadSectionData(BinaryReader reader, PifSectionHead sectionHead)
    {
        switch (sectionHead.DataLength)
        {
            case 0x68:
                Windows3X386 = Fill<Windows3x386>(reader);
                break;

            case 0x6:
                Windows3X286 = Fill<Windows3x286>(reader);
                break;

            case 0x1AC:
                Windows4XVmm = Fill<Windows4xVmm>(reader);
                break;

            case 0x8E:
                WindowsNt3 = Fill<WindowsNt3>(reader);
                break;

            // case "CONFIG SYS 4.0":
            // case "AUTOEXECBAT 4.0":
            //     // length of section [varies]
            //     byte[] data = reader.ReadBytes(sectionHead.DataLength);
            //
            //     // OEM encoding not supported. translate to ASCII
            //     string content = Encoding.ASCII.GetString(data);
            //     Debug.WriteLine(content);
            //     break;
            default:
                var sysNaming = new string(sectionHead.Name).Trim('\0');
                if (string.Equals(sysNaming, "CONFIG  SYS 4.0", StringComparison.InvariantCulture))
                {
                    
                }
                else if (string.Equals(sysNaming, "AUTOEXECBAT 4.0", StringComparison.InvariantCulture))
                {
                    
                }
                reader.ReadBytes(sectionHead.DataLength);
                break;
        }
    }
}