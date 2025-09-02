using System.Diagnostics;
using SunFlower.Le.Headers.Le;
using SunFlower.Le.Headers;
using SunFlower.Ne.Services;
using System.Text;
using SunFlower.Le.Models.Le;
using Object = SunFlower.Le.Headers.Le.Object;

namespace SunFlower.Le.Services;

public class LeDumpManager : UnsafeManager
{
    private UInt32 _offset;

    public LeDumpManager(string path)
    {
        _isVirtualDriver = false;
        Initialize(path);
    }

    public MzHeader MzHeader { get; set; }
    public LeHeader LeHeader { get; set; }
    public VxdHeader DriverHeader { get; set; }
    public List<Name> ResidentNames { get; set; } = [];
    public List<Name> NonResidentNames { get; set; } = [];
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];
    public List<EntryBundle> EntryBundles { get; set; } = [];
    public List<Object> Objects { get; set; } = [];
    public List<ObjectPageModel> ObjectPages { get; set; } = [];
    public List<uint> FixupPagesOffsets { get; set; } = [];
    public List<FixupRecordsTableModel> FixupRecords { get; set; } = [];
    public VxdDescriptionBlock DescriptionBlock { get; set; } = new();
    public VxdResources DriverResources { get; set; }
    public Win32Resource VersionInfo { get; set; }
    public FixedFileInfo FixedFileInfo { get; set; }
    private UInt32 Offset(UInt32 address) => _offset + address;
    private bool _isVirtualDriver;
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

        if (LeHeader.LE_ID is 0x584c or 0x4c58) // LX magic/cigam. IBM OS/2 standard Object Format (OMF).
            goto __continue; // Format incompatibility warning?!
        
        if (LeHeader.LE_ID is not 0x454c and not 0x4c45) // LE magic or cigam. Win32s (WOW32) or VxD-model driver or Microsoft OS/2 OMF.
            throw new InvalidOperationException("Doesn't have 'LE' signature");
        
        // Windows Virtual Driver header
        _isVirtualDriver = true; // <-- DDB block wanted!
        DriverHeader = Fill<VxdHeader>(reader);
        
        if (DriverHeader.LE_WindowsResLength == 0)
            goto __continue; // <-- no resources here.
        
        reader.BaseStream.Seek(DriverHeader.LE_WindowsResOffset, SeekOrigin.Begin); // |<-- save position
        DriverResources = Fill<VxdResources>(reader);
        stream.Position -= 2;
        //VersionInfo = ReadResourceBlock(reader, (uint)stream.Position);
        
        // EntryPoints table contains DDB entry.
        // DDB entry "Description block" has own offset. BUT firstly 
        // you MUST read EntryPoint table.
        
        // If OS/2 module (LX) - you better avoid specific driver structs
        // Some tables in 2 specifications really are the same. But NOT FOR LONG.
        __continue:
        FillNames(reader);
        FillImportingNames(reader);
        FillEntryPoints(reader);
        FillObjectTable(reader);
        FillObjectMap(reader);
        FillFixupPages(reader);
        
        if (_isVirtualDriver)
            FillDescriptionBlock(reader);
        
    }
    // Win32 Resource blocks handles in plugin like new type of Region
    // DataTable instead.
    //
    // ### Win32 Resource script (by 0x0ffset in <project_name>) 
    //
    // Usually this file contains more detailed information about
    // application's or driver's image. 
    // > ![WARNING]
    // > This resource script is a C/++ pseudocode
    // ```cpp
    // #include <winver.h>
    // ...
    // BLOCK "VersionInfo" {
    //      ...
    //      BLOCK "StringFileInfo" {
    //          BLOCK "040904e4b0" { <-- language ID???
    //              VALUE "CompanyName", "Microsoft Corporation\000",
    //              VALUE "OriginalFilename", "vjoyd.vxd"
    //              ...
    //          }
    //      }
    //      BLOCK VarFileInfo { VALUE "LanguageID", 1033 }
    // }
    // ```
    private Win32Resource ReadResourceBlock(BinaryReader reader, uint baseOffset)
    {
        var startOffset = reader.BaseStream.Position;
        var block = new Win32Resource();

        // resource header
        block.Type = reader.ReadUInt16();
        block.Length = reader.ReadUInt16();
        block.ValueLength = reader.ReadUInt16();

        // key T-CHAR -> ASCIIZ (Windows 95 not supports UNICODE)
        block.Key = ReadAsciiString(reader);
        reader.ReadUInt16(); // <-- padding?
        AlignToDword(reader);

        // value?
        if (block.ValueLength > 0)
        {
            block.Value = reader.ReadBytes(block.ValueLength);
            AlignToDword(reader);
        }

        // rec reader for other blocks
        var endOffset = startOffset + block.Length;
        while (reader.BaseStream.Position < endOffset)
        {
            var child = ReadResourceBlock(reader, baseOffset);
            block.Children.Add(child);
        }

        return block;
    }
    private string ReadUnicodeString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        byte b1, b2;
        while (true)
        {
            b1 = reader.ReadByte();
            b2 = reader.ReadByte();
            if (b1 == 0 && b2 == 0) break;
            bytes.Add(b1);
            bytes.Add(b2);
        }
        return Encoding.Unicode.GetString(bytes.ToArray());
    }
    private string ReadAsciiString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        byte b = reader.ReadByte();
        while (b != 0)
        {
            bytes.Add(b);
            b = reader.ReadByte();
        }

        return Encoding.ASCII.GetString(bytes.ToArray());
    }
    private void AlignToDword(BinaryReader reader)
    {
        var position = reader.BaseStream.Position;
        var alignment = (4 - (position % 4)) % 4;
        if (alignment > 0)
        {
            reader.ReadBytes((int)alignment);
        }
    }
    
    private void FillNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ResidentNames);
        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            ResidentNames.Add(new()
            {
                Size = i,
                String = name,
                Ordinal = ordinal
            });
        }

        // no Offset for not-resident names.
        reader.BaseStream.Position = LeHeader.LE_NoneRes;
        if (LeHeader.LE_NoneResSize == 0)
            return;

        byte j;
        while ((j = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(j));
            var ordinal = reader.ReadUInt16();
            NonResidentNames.Add(new Name
            {
                Size = j,
                String = name,
                Ordinal = ordinal
            });
        }
    }

    private void FillImportingNames(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(LeHeader.LE_ImportModNames);

        var size = reader.ReadByte();
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
    
    
    // View of EntryTable like Win16-OS/2 executables (NE)
    // ### 16-bit Entry Bundle #\{counter}  <--+
    // Target object - `\{objIndex}`           |    *** Nested dependent Data *** 
    // Flags                                   | BundleInfoStrings : List<String>
    //   - 16-bit/32-bit                       | Will be as independent strings collection
    //   - .VALID/.UNUSED                      | Fills EntryTables : List<DataTable> together
    // Count: 2                             <--+
    // |---|---------|-----------|--------| <--+
    // | # | Entry   | Object    | Offset |    | EntryTable in EntryTables : List<DataTable>
    // |---|---------|-----------|--------|    | Iterates with BundleInfoStrings
    // | 1 | EXPORT  | SHARED    | 1EBD20 |    |
    // | 2 | STATIC  | IMPURE    | 386A   | <--+ # - Global iterator or Ordinal setter. @1...@end
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
        var currentOrdinal = 1; // global exports iterator.
        
        while ((bundleSize = reader.ReadByte()) != 0)
        {
            var bundleFlags = reader.ReadByte(); 
            var objectIndex = reader.ReadUInt16();

            EntryBundle bundle = new(bundleSize, bundleFlags, objectIndex);

            var is32Bit = (bundleFlags & 0b00000010) != 0;

            List<Entry> entries = [];
            for (var i = 0; i < bundleSize; i++)
            {
                var entryFlags = reader.ReadByte();
                var offset = is32Bit ? reader.ReadUInt32() : reader.ReadUInt16();
                
                entries.Add(new Entry(currentOrdinal, entryFlags, offset));
                currentOrdinal++;
            }

            bundle.Entries = entries.ToArray();
            
            EntryBundles.Add(bundle);
        }
    }

    /// <summary>
    /// VxD model contains _DDB block
    /// I need this block
    /// </summary>
    /// <param name="reader"></param>
    private void FillDescriptionBlock(BinaryReader reader)
    {
        var ddbNonResident = NonResidentNames.FirstOrDefault(x => x.String.Contains("DDB"));
        if (ddbNonResident == null)
        {
            Debug.WriteLine("DDB not found in non-resident names");
            return;
        }
        var ddbOrdinal = ddbNonResident.Ordinal;
        
        var ddbEntry = EntryBundles
            .SelectMany(b => b.Entries)
            .FirstOrDefault(e => e.Ordinal == ddbOrdinal);

        if (ddbEntry == null)
        {
            Debug.WriteLine("DDB entry not found in entry bundles");
            return;
        }
        var bundleContainingDdb = EntryBundles.First(b => b.Entries.Contains(ddbEntry));
        var objectIndex = bundleContainingDdb.ObjectIndex;
        var ddbOffset = ddbEntry.Offset; // VA in object
        var objectEntry = Objects[objectIndex - 1];
        
        var pageSize = LeHeader.LE_PageSize;
        var pageIndexInObject = ddbOffset / pageSize;
        var offsetInPage = ddbOffset % pageSize;
        
        var pageMapIndex = objectEntry.PageMapIndex + pageIndexInObject;
        if (pageMapIndex >= ObjectPages.Count)
        {
            Debug.WriteLine("Page map index out of range");
            return;
        }
        var pageEntry = ObjectPages[(int)pageMapIndex];
        
        var physicalPageNumber = (uint)((pageEntry.Page.HighPage << 8) | pageEntry.Page.LowPage);
        var physicalOffset = (physicalPageNumber - 1) * pageSize + offsetInPage + Offset(LeHeader.LE_Data);
        
        reader.BaseStream.Position = physicalOffset;
        VxdDescriptionBlock block = Fill<VxdDescriptionBlock>(reader);
        DescriptionBlock = block;
    }
    private void FillObjectTable(BinaryReader reader)
    {
        // There;s no sections yet.
        // Here uses something little same about file segment and logical partition
        // of file. It's called Object in OS2_OMF_Format_And_Linear_Executable.pdf and eComStation_LE_Manual.pdf
    
        reader.BaseStream.Position = Offset(LeHeader.LE_ObjOffset);
        
        for (var i = 0; i < LeHeader.LE_ObjNum; i++)
        {
            var virtualSegmentSize = reader.ReadUInt32();
            var relocationBase = reader.ReadUInt32();
            var objectFlagsMask = reader.ReadUInt32();
            var pageMap = reader.ReadUInt32();
            var pageMapEntries = reader.ReadUInt32();
            var unknownField = reader.ReadUInt32();
            
            Objects.Add(new Object(
                virtualSegmentSize, 
                relocationBase,
                objectFlagsMask, 
                pageMap, 
                pageMapEntries, 
                unknownField)
            );
        }

    }
    private void FillObjectMap(BinaryReader reader)
    {
        long tablePosition = Offset(LeHeader.LE_PageMap);
        reader.BaseStream.Position = tablePosition;
        
        for (var i = 0; i < LeHeader.LE_Pages; i++)
        {
            var entry = new ObjectPage
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
    private void FillObjectPage(ObjectPage entry)
    {
        List<string> flags = [];
        
        switch (entry.Flags & (byte)ObjectPage.PageFlags.TypeMask)
        {
            case (byte)ObjectPage.PageFlags.Legal:
                flags.Add("OBJPAGE_LEGAL");
                break;
            case (byte)ObjectPage.PageFlags.Iterated:
                flags.Add("OBJPAGE_ITERATED");
                break;
            case (byte)ObjectPage.PageFlags.Invalid:
                flags.Add("OBJPAGE_INVALID");
                break;
            case (byte)ObjectPage.PageFlags.ZeroFilled:
                flags.Add("OBJPAGE_BSS");
                break;
        }
    
        if ((entry.Flags & (byte)ObjectPage.PageFlags.LastPageInFile) != 0)
        {
            flags.Add("OBJPAGE_LAST");
        }
        
        ObjectPages.Add(new ObjectPageModel(entry, flags));
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
        
        var tableStart = LeHeader.LE_ImportModNames;
        reader.BaseStream.Position = tableStart;
        // Пропускаем записи от 0 до moduleIndex
        for (var i = 0; i <= moduleIndex; i++)
        {
            var len = reader.ReadByte();
            if (len == 0)
            {
                name = string.Empty;
            }
            else
            {
                var bytes = reader.ReadBytes(len);
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
        var fixupOffset = startOffset; // + Offset(FixupRec)
        
        reader.BaseStream.Position = fixupOffset;
        var endPosition = (long)fixupOffset + fixupSize;
        
        while (reader.BaseStream.Position < endPosition)
        {
            var atp = new List<string>();
            var rtp = new List<string>();
            var importingName = string.Empty;
            var importingOrdinal = string.Empty;
            
            var record = new FixupRecord
            {
                AddressType = reader.ReadByte(),
                RelocationType = reader.ReadByte()
            };

            // Address type flags
            var hasMultipleOffsets = (record.AddressType & 0x20) != 0;
            var hasOffsetList = (record.AddressType & 0x02) != 0;
            
            if (hasMultipleOffsets)
            {
                atp.Add("ADDR_MULTI_OFFSETS");
                var count = reader.ReadByte();
                record.Offsets = new ushort[count];
                for (var i = 0; i < count; i++)
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

                    var procedureName = "{BEFORE_READ}";
                    try
                    {
                        var procTableStart = LeHeader.LE_ImportNames;
                        reader.BaseStream.Position = procTableStart + record.NameOffset;
        
                        var nameLength = reader.ReadByte();
                        if (nameLength > 0) // Length
                        {
                            var nameBytes = reader.ReadBytes(nameLength);
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
                        var current = reader.BaseStream.Position;
                        var moduleName = GetModuleName(reader, record.ModuleIndex);
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
        for (var i = 0; i <= LeHeader.LE_Pages; i++)
        {
            FixupPagesOffsets.Add(reader.ReadUInt32());
        }
        
        for (var pageIdx = 0; pageIdx < LeHeader.LE_Pages; pageIdx++)
        {
            var start = FixupPagesOffsets[pageIdx];
            var end = FixupPagesOffsets[pageIdx + 1];
            var size = end - start;
    
            if (size > 0)
            {
                ReadFixupRecordsTable(reader, size, start, pageIdx);
            }
        }
    }
}