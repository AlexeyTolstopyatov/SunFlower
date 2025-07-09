using SunFlower.Le.Headers.Le;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Services;
using System.Text;
using SunFlower.Le.Models;

namespace SunFlower.Le.Services;

public class LeDumpManager : UnsafeManager
{
    private UInt32 _offset;

    public LeDumpManager(string path)
    {
        Initialize(path);
    }

    public MzHeader MzHeader { get; set; } = new();
    public LeHeader LeHeader { get; set; } = new();
    public VddHeader DriverHeader { get; set; } = new();
    public List<ResidentName> ResidentNames { get; set; } = [];
    public List<NonResidentName> NonResidentNames { get; set; } = [];
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];
    public List<EntryBundle> EntryBundles { get; set; } = [];
    public List<ObjectTableModel> ObjectTables { get; set; } = [];
    public List<ObjectPageModel> ObjectPages { get; set; } = [];
    public UInt32 Offset(UInt32 address) => _offset + address;

    private void Initialize(string path)
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        MzHeader = Fill<MzHeader>(reader);

        if (MzHeader.e_sign != 0x5a4d && MzHeader.e_sign != 0x4d5a) // cigam is very old sign but it also exists
            throw new InvalidOperationException("Doesn't have DOS/2 signature");

        _offset = MzHeader.e_lfanew;
        stream.Position = _offset;

        LeHeader = Fill<LeHeader>(reader);

        if (LeHeader.LE_ID != 0x454c && LeHeader.LE_ID != 0x4c45) // magic or cigam
            throw new InvalidOperationException("Doesn't have 'LE' signature");

        stream.Seek(20, SeekOrigin.Current); // skip zero-filled page

        DriverHeader = Fill<VddHeader>(reader);
        FillNames(reader);
        FillImportingNames(reader);
        FillEntryPoints(reader);
        FillObjectTable(reader);
        FillObjectMap(reader);
    }

    private void FillNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ResidentNames);
        byte size = reader.ReadByte();
        while (size != 0)
        {
            ResidentName name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Ordinal = reader.ReadUInt16(),
                Size = size
            };
            ResidentNames.Add(name);
            size = reader.ReadByte();
        }

        reader.BaseStream.Position = LeHeader.LE_NoneRes;
        size = reader.ReadByte();

        while (size != 0)
        {
            NonResidentName name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Ordinal = reader.ReadUInt16(),
                Size = size
            };
            NonResidentNames.Add(name);
            size = reader.ReadByte();
        }
    }

    private void FillImportingNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ImportModNames);

        byte size = reader.ReadByte();
        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingModules.Add(name);
            size = reader.ReadByte();
        }

        reader.BaseStream.Position = Offset(LeHeader.LE_ImportNames);
        size = reader.ReadByte();

        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingProcedures.Add(name);
            size = reader.ReadByte();
        }
    }
    
    /// <summary>
    /// If EntryPoints exists, some of them stores in <see cref="ResidentNames"/>
    /// , some of them in <see cref="NonResidentNames"/>, and their Ordinals in
    /// Non/Resident tables are pointers to EntryTable. (they tell "Where in EntryTable entry stores")
    /// </summary>
    /// <param name="reader"></param>
    private void FillEntryPoints(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_EntryTable);

        byte bundleSize;
        while ((bundleSize = reader.ReadByte()) != 0)
        {
            byte bundleFlags = reader.ReadByte(); 
            ushort objectIndex = reader.ReadUInt16();
            
            EntryBundle bundle = new()
            {
                EntriesCount = bundleSize,
                EntryBundleIndex = bundleFlags,
                ObjectIndex = objectIndex,
            };

            bool is32Bit = (bundleFlags & 0b00000010) != 0;

            List<Entry16> entries = [];
            List<Entry32> entry32S = [];
            for (int i = 0; i < bundleSize; i++)
            {
                byte entryFlags = reader.ReadByte();

                if (is32Bit)
                {
                    uint offset = reader.ReadUInt32();
                    entry32S.Add(new Entry32
                    {
                        Flag = entryFlags,
                        Offset = offset
                    });
                }
                else
                {
                    ushort offset = reader.ReadUInt16();
                    entries.Add(new Entry16
                    {
                        Flag = entryFlags,
                        Offset = offset
                    });
                }
            }

            bundle.Entries = entries.ToArray();
            bundle.ExtendedEntries = entry32S.ToArray();

            EntryBundles.Add(bundle);
        }
    }
    
    private void FillObjectTable(BinaryReader reader)
    {
        // There;s no sections yet.
        // Here uses something little same about file segment and logical partition
        // of file. It's called Object in OS2_OMF_Format_And_Linear_Executable.pdf and eComStation_LE_Manual.pdf
    
        reader.BaseStream.Position = Offset(LeHeader.LE_ObjOffset);
    
        List<ObjectTable> objectTable = [];

        for (int i = 0; i < LeHeader.LE_ObjNum; i++)
        {
            ObjectTable entry = new()
            {
                VirtualSegmentSize = reader.ReadUInt32(),
                RelocationBaseAddress = reader.ReadUInt32(),
                ObjectFlags = reader.ReadUInt32(),
                PageMapIndex = reader.ReadUInt32(),
                PageMapEntries = reader.ReadUInt32(),
                Unknown = reader.ReadUInt32()
            };
        
            objectTable.Add(entry);
            DecodeObjectFlags(entry);
        }

    }
    
    private void DecodeObjectFlags(ObjectTable entry)
    {
        // Permissions
        string readable = ((entry.ObjectFlags & 0x0001) != 0) ? "OBJ_READ" : "";
        string writable = ((entry.ObjectFlags & 0x0002) != 0) ? "OBJ_WRITE" : "";
        string executable = ((entry.ObjectFlags & 0x0004) != 0) ? "OBJ_EXEC" : "";
        string isResource = ((entry.ObjectFlags & 0x0008) != 0) ? "OBJ_RES" : "";
        
        // Object type
        uint objectType = (entry.ObjectFlags >> 8) & 0x03;
        string typeDesc = objectType switch
        {
            0 => "OBJ_TYPE_NORMAL",
            1 => "OBJ_TYPE_ZERO_FILLED",
            2 => "OBJ_TYPE_RESIDENT",
            3 => "OBJ_TYPE_RESIDENT_CONTIGUOUS",
            _ => "OBJ_UNKNOWN"
        };
    
        // Specific flags
        string is16x16 = ((entry.ObjectFlags & 0x1000) != 0) ? "OBJ_16_16_ALIAS" : "";
        string isBig = ((entry.ObjectFlags & 0x2000) != 0) ? "OBJ_USE32" : "OBJ_USE16";
        string isConforming = ((entry.ObjectFlags & 0x4000) != 0) ? "OBJ_CONFORM" : "";
        string hasIOPrev = ((entry.ObjectFlags & 0x2000) != 0) ? "OBJ_IO_PRIVILEGE" : "";
        
        List<string> flags
            = [readable, writable, executable, isResource, typeDesc, isBig, is16x16, hasIOPrev, isConforming];
        flags = flags.Where(s => !string.IsNullOrEmpty(s)).ToList();
        
        ObjectTables.Add(new ObjectTableModel(entry, flags));
    }

    private void FillObjectMap(BinaryReader reader)
    {
        long tablePosition = Offset(LeHeader.LE_PageMap);
        reader.BaseStream.Position = tablePosition;
        
        //List<ObjectPage> entries = new();
        
        for (int i = 0; i < LeHeader.LE_Pages; i++)
        {
            ObjectPage entry = new ObjectPage
            {
                HighPage = reader.ReadUInt16(),
                LowPage = reader.ReadByte(),
                Flags = reader.ReadByte()
            };
            // alignment skip
            reader.ReadUInt32(); 
            
            FillObjectPage(entry);
        }

    }
    private long CalculatePageFileOffset(ObjectPage entry)
    {
        uint pageNumber = (uint)((entry.HighPage << 8) | entry.LowPage);
        
        // PageOffset = (Page# - 1) * sizeof(Page) + DataPageOffset
        return (pageNumber - 1) * LeHeader.LE_PageSize 
               + LeHeader.LE_Data 
               + MzHeader.e_lfanew;
    }
    private void FillObjectPage(ObjectPage entry)
    {
        List<string> flags = [];
        
        switch (entry.Flags & (byte)ObjectPage.PageFlags.TypeMask)
        {
            case (byte)ObjectPage.PageFlags.Legal:
                flags.Add("DATA32 or CODE32");
                break;
            case (byte)ObjectPage.PageFlags.Iterated:
                flags.Add("compressed or repeated data");
                break;
            case (byte)ObjectPage.PageFlags.Invalid:
                flags.Add("should be skipped");
                break;
            case (byte)ObjectPage.PageFlags.ZeroFilled:
                flags.Add("uninitialized data");
                break;
        }
    
        if ((entry.Flags & (byte)ObjectPage.PageFlags.LastPageInFile) != 0)
        {
            flags.Add("last page");
        }
        
        ObjectPages.Add(new ObjectPageModel(entry, flags, CalculatePageFileOffset(entry)));
    }
}