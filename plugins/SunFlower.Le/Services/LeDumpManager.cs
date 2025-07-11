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

    public MzHeader MzHeader { get; set; }
    public LeHeader LeHeader { get; set; }
    public VddHeader DriverHeader { get; set; }
    public List<ResidentName> ResidentNames { get; set; } = [];
    public List<NonResidentName> NonResidentNames { get; set; } = [];
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];
    public List<EntryBundleModel> EntryBundles { get; set; } = [];
    public List<ObjectTableModel> ObjectTables { get; set; } = [];
    public List<ObjectPageModel> ObjectPages { get; set; } = [];
    public List<uint> FixupPagesOffsets { get; set; } = [];
    public List<FixupRecordsTableModel> FixupRecords { get; set; } = [];
    public UInt32 Offset(UInt32 address) => _offset + address;

    /// <summary>
    /// Дверь сартира
    /// </summary>
    public Dictionary<ushort, string> NamesCache { get; } = new();

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

        if (LeHeader.LE_ID is 0x584c or 0x4c58)
            goto __continue; // Format incompatibility warning?!
        
        if (LeHeader.LE_ID is not 0x454c and not 0x4c45) // magic or cigam
            throw new InvalidOperationException("Doesn't have 'LE' signature");
        
        stream.Seek(20, SeekOrigin.Current); // skip zero-filled page
        DriverHeader = Fill<VddHeader>(reader);

        __continue:
        FillNames(reader);
        FillImportingNames(reader);
        FillEntryPoints(reader);
        FillObjectTable(reader);
        FillObjectMap(reader);
        FillFixupPages(reader);
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
        int count = 1;
        while ((bundleSize = reader.ReadByte()) != 0)
        {
            count++;
            
            byte bundleFlags = reader.ReadByte(); 
            ushort objectIndex = reader.ReadUInt16();
            
            EntryBundle bundle = new()
            {
                EntriesCount = bundleSize,
                EntryBundleIndex = bundleFlags,
                ObjectIndex = objectIndex,
            };

            bool is32Bit = ((bundleFlags & 0b00000010) != 0);

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
            
            EntryBundles.Add(new EntryBundleModel(count, bundle, flags));
        }
    }
    
    private void FillObjectTable(BinaryReader reader)
    {
        // There;s no sections yet.
        // Here uses something little same about file segment and logical partition
        // of file. It's called Object in OS2_OMF_Format_And_Linear_Executable.pdf and eComStation_LE_Manual.pdf
    
        reader.BaseStream.Position = Offset(LeHeader.LE_ObjOffset);
        
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
        string is16By16 = ((entry.ObjectFlags & 0x1000) != 0) ? "OBJ_16_16_ALIAS" : "";
        string isBig = ((entry.ObjectFlags & 0x2000) != 0) ? "OBJ_USE32" : "OBJ_USE16";
        string isConforming = ((entry.ObjectFlags & 0x4000) != 0) ? "OBJ_CONFORM" : "";
        string hasIoPrev = ((entry.ObjectFlags & 0x2000) != 0) ? "OBJ_IO_PRIVILEGE" : "";
        
        List<string> flags
            = [readable, writable, executable, isResource, typeDesc, isBig, is16By16, hasIoPrev, isConforming];
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
                flags.Add("OBJPAGE_CODE_OR_DATA");
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

    private readonly Dictionary<uint, string> _moduleNameCache = [];
    private string GetModuleName(BinaryReader reader, byte moduleIndex)
    {
        if (moduleIndex >= LeHeader.LE_ImportModNum)
            return $"<InvalidIndex_#{moduleIndex}>";
        if (moduleIndex == 0)
            return string.Empty;
        if (_moduleNameCache.TryGetValue(moduleIndex, out var name))
            return name;
        
        uint tableStart = LeHeader.LE_ImportModNames;
        reader.BaseStream.Position = tableStart;
        // Пропускаем записи от 0 до moduleIndex
        for (int i = 0; i <= moduleIndex; i++)
        {
            byte len = reader.ReadByte();
            if (len == 0)
            {
                name = string.Empty;
            }
            else
            {
                byte[] bytes = reader.ReadBytes(len);
                name = Encoding.ASCII.GetString(bytes);
            }
            // only if single address
            if (i == moduleIndex)
            {
                _moduleNameCache[moduleIndex] = name;
                return name;
            }
        }
        return $"<ReadError_#{moduleIndex}>";
    }

    /// <summary>
    /// Every fixup record you can find -- use FillFixupPages
    /// because fixup pages table tells characteristics of record
    /// </summary>
    /// <param name="reader"><see cref="BinaryReader"/> instance</param>
    /// <param name="fixupSize">Size proceed in FillFixupPages for current record</param>
    /// <param name="startOffset">Where address of start record</param>
    /// <param name="pageIndex"></param>
    /// <returns>Record and writes it to Record model collection</returns>
    private void ReadFixupRecordsTable(BinaryReader reader, uint fixupSize, uint startOffset, int pageIndex)
    {
        uint fixupOffset = startOffset; // + Offset(FixupRec)
        
        reader.BaseStream.Position = fixupOffset;
        long endPosition = (long)fixupOffset + fixupSize;
        
        while (reader.BaseStream.Position < endPosition)
        {
            List<string> atp = new List<string>();
            List<string> rtp = new List<string>();
            string importingName = string.Empty;
            string importingOrdinal = string.Empty;
            
            FixupRecord record = new FixupRecord
            {
                AddressType = reader.ReadByte(),
                RelocationType = reader.ReadByte()
            };

            // Address type flags
            bool hasMultipleOffsets = (record.AddressType & 0x20) != 0;
            bool hasOffsetList = (record.AddressType & 0x02) != 0;
            
            if (hasMultipleOffsets)
            {
                atp.Add("ADDR_MULTI_OFFSETS");
                byte count = reader.ReadByte();
                record.Offsets = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    record.Offsets[i] = reader.ReadUInt16();
                }
            }
            else if (hasOffsetList)
            {
                atp.Add("ADDR_SINGLE_OFFSET");
                record.Offsets = new[] { reader.ReadUInt16() };
            }
            else
            {
                atp.Add("ADDR_NO_OFFSETS");
                record.Offsets = [];
            }

            // Обработка Relocation Type
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

                    string procedureName = "{BEFORE_READ}";
                    try
                    {
                        uint procTableStart = LeHeader.LE_ImportNames;
                        reader.BaseStream.Position = procTableStart + record.NameOffset;
        
                        byte nameLength = reader.ReadByte();
                        if (nameLength > 0) // Length
                        {
                            byte[] nameBytes = reader.ReadBytes(nameLength);
                            procedureName = Encoding.ASCII.GetString(nameBytes); // or IBM-850
                        }
                    }
                    catch
                    {
                        procedureName = "READ_ERROR";
                    }

                    if (LeHeader.LE_ImportModNum == 0)
                    {
                        // beautiful mind
                        if (record.ModuleIndex == 0x00)
                            importingName = $"[?KERNEL]!{procedureName}";
                        else if (record.ModuleIndex == 0x01)
                            importingName = $"[?GDI]!{procedureName}";
                        else
                            importingName = $"MODULE_#{record.ModuleIndex:X2}!{procedureName}";
                    }
                    else
                    {
                        long current = reader.BaseStream.Position;
                        string moduleName = GetModuleName(reader, record.ModuleIndex);
                        importingName = $"{moduleName}!{procedureName}";

                        reader.BaseStream.Position = current;
                    }
                    break;
                    
                case "REL_OSFIXUP": // OS/2 is hiding something interesting
                    rtp.Add("REL_OSFIXUP");
                    record.OsFixup = reader.ReadByte();
                    break;
            }

            // Extra fields
            if (FixupRelocationType.IsAdditive(record.RelocationType))
            {
                rtp.Add("REL_ADDITIVE");
                record.AddValue = FixupRelocationType.Is32BitTarget(record.RelocationType)
                    ? reader.ReadInt32()
                    : reader.ReadInt16();
            }
            
            if (FixupRelocationType.HasExtraData(record.RelocationType))
            {
                rtp.Add("REL_EXTRA_DATA");
                record.ExtraData = reader.ReadUInt16();
            }

            FixupRecordsTableModel model = new(pageIndex, record, atp, rtp, importingName, importingOrdinal);
            FixupRecords.Add(model);
        }
    }

    private void FillFixupPages(BinaryReader reader)
    {
        // Offset(x) = returns x + HeaderOffset
        reader.BaseStream.Position = Offset(LeHeader.LE_Fixups);
        // MemoryPagesCount has value of fixup .EXE pages
        for (int i = 0; i <= LeHeader.LE_Pages; i++)
        {
            FixupPagesOffsets.Add(reader.ReadUInt32());
        }
        
        for (int pageIdx = 0; pageIdx < LeHeader.LE_Pages; pageIdx++)
        {
            uint start = FixupPagesOffsets[pageIdx];
            uint end = FixupPagesOffsets[pageIdx + 1];
            uint size = end - start;
    
            if (size > 0)
            {
                ReadFixupRecordsTable(reader, size, start, pageIdx);
            }
        }
    }
}