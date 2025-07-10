using System.ComponentModel;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Headers;
using SunFlower.Ne.Services;
using System.Text;
using SunFlower.Le.Models.Le;

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
    public List<EntryBundleModel> EntryBundles { get; set; } = [];
    public List<ObjectTableModel> ObjectTables { get; set; } = [];
    public List<ObjectPageModel> ObjectPages { get; set; } = [];
    public List<uint> FixupPagesOffsets { get; set; }
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
        List<string> flags = [];
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

            bool is32Bit = ((bundleFlags & 0b00000010) != 0);
            bool isVaild = ((bundleFlags & 0b00000001) != 0);

            List<Entry16> entries = [];
            List<Entry32> entry32S = [];
            for (int i = 0; i < bundleSize; i++)
            {
                byte entryFlags = reader.ReadByte();

                if (is32Bit)
                {
                    flags.Add("ENTRY_USE32");
                    uint offset = reader.ReadUInt32();
                    
                    string isExported = ((entryFlags & 0x01) != 0) ? "ENTRY_EXPORT" : "";
                    string isSharedObj = ((entryFlags & 0x02) != 0) ? "OBJ_SHARED" : "OBJ_LOCAL";

                    entry32S.Add(new Entry32
                    {
                        Flag = entryFlags,
                        Offset = offset,
                        FlagNames = new[] {isExported, isSharedObj}.Where(s => !string.IsNullOrEmpty(s)).ToArray()
                    });
                }
                else
                {
                    // fill flags for entries-bundle
                    flags.Add("ENTRY_USE16");
                    ushort offset = reader.ReadUInt16();
                    string isExported = ((entryFlags & 0x01) != 0) ? "ENTRY_EXPORT" : "";
                    string isSharedObj = ((entryFlags & 0x02) != 0) ? "OBJ_SHARED" : "OBJ_LOCAL";
                    
                    // fill flags for each entry
                    
                    entries.Add(new Entry16
                    {
                        Flag = entryFlags,
                        Offset = offset,
                        FlagNames = new[] {isExported, isSharedObj}.Where(x => !string.IsNullOrEmpty(x)).ToArray()
                    });
                }
            }

            bundle.Entries = entries.ToArray();
            bundle.ExtendedEntries = entry32S.ToArray();
            
            EntryBundles.Add(new EntryBundleModel(bundle, flags));
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
                flags.Add("OBJPAGE_LEGAL");
                break;
            case (byte)ObjectPage.PageFlags.Iterated:
                flags.Add("OBJPAGE_ITER (compressed or repeated data)");
                break;
            case (byte)ObjectPage.PageFlags.Invalid:
                flags.Add("OBJPAGE_INVALID (should be skipped)");
                break;
            case (byte)ObjectPage.PageFlags.ZeroFilled:
                flags.Add("OBJPAGE_ZERO (uninitialized data)");
                break;
        }
    
        if ((entry.Flags & (byte)ObjectPage.PageFlags.LastPageInFile) != 0)
        {
            flags.Add("OBJPAGE_LAST");
        }
        
        ObjectPages.Add(new ObjectPageModel(entry, flags, CalculatePageFileOffset(entry)));
    }
    /// <summary>
    /// Every fixup record you can find -- use FillFixupPages
    /// because fixup pages table tells characteristics of record
    /// </summary>
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <param name="fixupSize">Size proceed in FillFixupPages for current record</param>
    /// <returns>Record and writes it to Record model collection</returns>
    public List<FixupRecord> ReadFixupRecordsTable(BinaryReader reader, uint fixupSize)
    {
        // prepare fixup record addresses
        uint fixupOffset = Offset(LeHeader.LE_FixupsRec);
        
        reader.BaseStream.Position = fixupOffset;
        List<FixupRecord> records = [];
        long endPosition = fixupOffset + fixupSize;
        
        // prepare translator collections 
        List<string> atp = [];
        List<string> rtp = [];
        string importingName = string.Empty;
        string importingOrdinal = string.Empty;
        
        while (reader.BaseStream.Position < endPosition)
        {
            // address type
            FixupRecord record = new FixupRecord
            {
                AddressType = reader.ReadByte(),
                RelocationType = reader.ReadByte()
            };
            
            
            if (FixupRelocationAddressType.HasOffsetList(record.AddressType))
            {
                atp.Add("ADDR_HAS_OFFSETS");
                
                byte count = reader.ReadByte();
                record.Offsets = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    record.Offsets[i] = reader.ReadUInt16();
                }
            }
            else
            {
                atp.Add("ADDR_NO_OFFSETS");
                record.Offsets = new[] { reader.ReadUInt16() };
            }
            // relocation type
            switch (FixupRelocationType.GetRelocationType(record.RelocationType))
            {
                case "REL_INTERNAL_REF":
                    rtp.Add("REL_INTERNAL_REF");
                    record.TargetObject = reader.ReadByte();
                    
                    break;
                    
                case "REL_IMPORT_ORD":
                    rtp.Add("REL_IMPORT_ORD");
                    record.ModuleIndex = reader.ReadByte();
                    record.Ordinal = FixupRelocationType.Is16BitOrdinal(record.RelocationType) 
                        ? reader.ReadUInt16() 
                        : reader.ReadByte();

                    importingOrdinal = "@" + record.Ordinal;
                    
                    break;
                    
                case "REL_IMPORT_NAME":
                    
                    rtp.Add("REL_IMPORT_NAME");
                    record.ModuleIndex = reader.ReadByte();
                    reader.ReadByte(); // Reserved
                    record.NameOffset = reader.ReadUInt16();

                    long currentPosition = reader.BaseStream.Position;
                    
                    // black magic procedure names tricks
                    reader.BaseStream.Position = (long)(Offset(LeHeader.LE_ImportNames) + record.NameOffset);
                    byte size = reader.ReadByte();
                    if (size != 0)
                    {
                        importingName = Encoding.ASCII.GetString(reader.ReadBytes(size));
                    }

                    reader.BaseStream.Position = currentPosition;
                    break;
            }

            // Extra fields checkout
            if (FixupRelocationType.IsAdditive(record.RelocationType))
            {
                if (FixupRelocationType.Is32BitTarget(record.RelocationType))
                {
                    reader.ReadUInt32();
                    rtp.Add("REL_TARGET_32");
                }
                else
                {
                    reader.ReadUInt16();
                    rtp.Add("REL_TARGET_16");
                }
            }
            
            if (FixupRelocationType.HasExtraData(record.RelocationType))
            {
                rtp.Add("REL_EXTRA_DATA");
                record.ExtraData = reader.ReadUInt16();
            }

            records.Add(record);
            
            FixupRecordsTableModel model = new(record, atp, rtp, importingName, importingOrdinal);
        }
        return records;
    }

    private void FillFixupPages(BinaryReader reader)
    {
        for (int i = 0; i <= LeHeader.; i++)
        {
            FixupPagesOffsets.Add(reader.ReadUInt32());
        }
    }
}