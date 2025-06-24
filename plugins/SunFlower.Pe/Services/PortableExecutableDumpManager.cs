using System.Globalization;
using System.Runtime.InteropServices;
using SunFlower.Pe.Headers;
using FileStream = System.IO.FileStream;

namespace SunFlower.Pe.Services;

public class PortableExecutableDumpManager(string path) : IManager
{
    public MzHeader Dos2Header { get; set; }
    public PeFileHeader FileHeader { get; set; }
    public PeOptionalHeader32 OptionalHeader32 { get; set; }
    public PeOptionalHeader OptionalHeader { get; set; }
    public PeDirectory[] PeDirectories { get; set; } = [];
    public PeSection[] PeSections { get; set; } = [];
    public PeImageExportDirectory ExportDirectory { get; set; }
    public bool Is64Bit { get; set; }

    /// <summary>
    /// Starts manager in another thread
    /// </summary>
    public void Initialize()
    {
        Task.Run(async() =>
        {
            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new(stream);

            await FindHeaders(reader);
            await FindSectionsTable(reader);
            
        });
    }

    private Task FindHeaders(BinaryReader reader)
    {
        UInt16 dos2 = reader.ReadUInt16();

        if (dos2 != 0x5A4D && dos2 != 0x4D5A)
        {
            return Task.FromException(new InvalidOperationException());
        }
        
        reader.BaseStream.Position = 0;
        MzHeader dos2Hdr = Fill<MzHeader>(reader);
        Dos2Header = dos2Hdr;

        reader.ReadUInt32();
        PeFileHeader fileHdr = Fill<PeFileHeader>(reader);
        FileHeader = fileHdr;

        // simple way to define required CPU word size
        Is64Bit = (FileHeader.Machine & 0x0100) == 0;

        if (Is64Bit)
        {
            OptionalHeader = Fill<PeOptionalHeader>(reader);
        }
        else
        {
            OptionalHeader32 = Fill<PeOptionalHeader32>(reader);
        }

        return Task.CompletedTask;
    }

    private Task FindSectionsTable(BinaryReader reader)
    {
        List<PeSection> sections = [];
        for (UInt32 i = 0; i < FileHeader.NumberOfSections; ++i)
        {
            sections.Add(Fill<PeSection>(reader));
        }

        PeSections = sections.ToArray();
        return Task.CompletedTask;
    }
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <typeparam name="TStruct">structure</typeparam>
    /// <returns></returns>
    private TStruct Fill<TStruct>(BinaryReader reader) where TStruct : struct
    {
        Byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(TStruct)));
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        TStruct result = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();
        
        return result;
    }
}