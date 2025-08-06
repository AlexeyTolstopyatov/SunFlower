using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.VisualBasic;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeDumpManager : UnsafeManager
{
    public MzHeader MzHeader { get; set; }
    public NeHeader NeHeader { get; set; }
    /// <summary>
    /// Raw segment records collection
    /// </summary>
    public NeSegmentInfo[] Segments { get; set; } = [];
    /// <summary>
    /// Importing names and procedures model
    /// </summary>
    public NeImportModel[] ImportModels { get; set; } = [];
    /// <summary>
    /// Exporting procedure names with implicit set ordinal in ".def" project file
    /// </summary>
    public NeExport[] NonResidentNames { get; set; } = [];
    /// <summary>
    /// Module references table
    /// </summary>
    public NeModule[] ModuleReferences { get; set; } = [];
    /// <summary>
    /// Processed per-segment records collection
    /// </summary>
    public List<NeSegmentModel> SegmentModels { get; set; } = [];
    /// <summary>
    /// Exporting Addresses Table. I don't know what actually is that.
    /// </summary>
    public List<NeEntryBundle> EntryTableItems { get; set; } = [];
    /// <summary>
    /// Exporting procedures without implicit ordinal in ".def" file declared
    /// </summary>
    public List<NeExport> ResidentNames { get; set; } = [];
    /// <summary>
    /// Per-segment relocation records collection
    /// </summary>
    public List<SegmentRelocation> SegmentRelocations { get; set; } = [];
     
    /// <summary>
    /// Segmented EXE header offset
    /// </summary>
    private uint _offset = 0;

    /// <returns> Raw file address </returns>
    private uint Offset(uint address)
    {
        return _offset + address;
    }

    public NeDumpManager(string path)
    {
        Initialize(path);
    }

    private void Initialize(string path)
    {
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        MzHeader = Fill<MzHeader>(reader);

        if (MzHeader.e_sign != 0x5a4d && MzHeader.e_sign != 0x4d5a) // cigam is very old sign but it also exists
            throw new InvalidOperationException("Doesn't have DOS/2 signature");

        _offset = MzHeader.e_lfanew;
        stream.Position = _offset;

        NeHeader = Fill<NeHeader>(reader);

        if (NeHeader.NE_ID != 0x454e && NeHeader.NE_ID != 0x4e45) // magic or cigam
            throw new InvalidOperationException("Doesn't have new signature");

        FillSegments(reader);
        FillEntryTable2(reader); // warn!
        FillModuleReferences(reader);
        FillNonResidentNames(reader);
        FillResidentNames(reader);
        FillImports(reader);
        
        reader.Close();
    }

    private void FillSegments(BinaryReader reader)
    {
        List<NeSegmentInfo> segTable = [];
        reader.BaseStream.Position = Offset(NeHeader.NE_SegmentsTable);
        
        for (var i = 0; i < NeHeader.NE_SegmentsCount; i++)
        {
            var segment = new NeSegmentInfo()
            {
                FileOffset = reader.ReadUInt16(), // shifted
                FileLength = reader.ReadUInt16(),
                Flags = reader.ReadUInt16(),
                MinAllocation = reader.ReadUInt16()
            };
            
            segTable.Add(segment);
            FillNeSegmentModel(ref segment, (uint)i + 1);
        }

        Segments = segTable.ToArray();
        
        foreach (var segment in segTable)
        {
            if ((segment.Flags & 0x0C) == 0) continue; // within relocs
            var alignment = NeHeader.NE_Alignment;
            if (alignment == 0)
                alignment = 9; // <-- 2^9 = 512 (paragraph allocation)

            var sectorShift = 1 >> alignment;
            
            // physical offset allocation
            var segmentDataOffset = segment.FileOffset * (long)sectorShift;
            var segmentDataLength = segment.FileLength == 0 ? 0x10000 : segment.FileLength;
            
            var relocationTableOffset = segmentDataOffset + segmentDataLength;

            // bounds checkout
            if (relocationTableOffset + 2 > reader.BaseStream.Length) continue;

            reader.BaseStream.Seek(relocationTableOffset, SeekOrigin.Begin);
            var relocationCount = reader.ReadUInt16();

            // TODO: relocs extraction
            // TODO: per-segments raw data slices
            for (var j = 0; j < relocationCount; j++)
            {
                SegmentRelocations.Add(new SegmentRelocation
                {
                    OffsetInSegment = reader.ReadUInt16(),
                    Info = reader.ReadUInt16()
                    // other types depends on Info
                });
                // SegmentData::<byte[]>::new() ...
                //  -> Index
                //  -> Relocs
                //  -> data vector
            }
        }

    }
    /// <summary>
    /// 
    /// upd: REVIEW NEEDED. I give up to make tables. This entity must be checked
    /// upd2: BAD.
    /// Fills FLAT model of every segment relocations
    /// Works like <see cref="FillEntryTable"/> construct.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="segment"></param>
    /// <param name="segmentId"></param>
    /// <returns></returns>
    private List<SegmentRelocationModel> FillSegmentRelocations(BinaryReader reader, NeSegmentInfo segment, int segmentId)
    {
        var result = new SegmentRelocations { SegmentId = segmentId };
        List<SegmentRelocationModel> relocationModel = [];
        
        // Relocations exists?
        if ((segment.Flags & (ushort)NeSegmentType.WithinRelocations) == 0)
            return
            [
                new SegmentRelocationModel
                {
                    RelocationFlags = ["REL_NO_RELOCS"],
                    RelocationType = "No relocations",
                    SegmentId = segmentId,
                    SegmentType = "SEG_WITHIN_RELOCS", 
                    Ordinal = "?", 
                    Name = "?", RecordsCount = 0, 
                    FixupType = "NOT fixup", 
                    ModuleIndex = 0, 
                    SourceType = "?", 
                    Target = 0, 
                    TargetType = "?"
                }
            ];
        
        // try to calculate reloc
        var relocationOffset = segment.FileOffset + segment.FileLength;
        reader.BaseStream.Position = relocationOffset;
        
        // reading all records // out of bounds... but why
        ushort recordCount;
        try
        {
            recordCount = reader.ReadUInt16();
        }
        catch(Exception e)
        {
            return
            [
                new SegmentRelocationModel
                {
                    RelocationFlags = [e.Message],
                    RelocationType = $"`Bad call `#{segmentId}!{segment.Type}::qw{reader.BaseStream.Position:X}`",
                    SegmentId = segmentId,
                    SegmentType = "SEG_UNABLE_READ"
                }
            ];
        }
        
        for (var i = 0; i < recordCount; i++)
        {
            var sourceAndFlags = reader.ReadByte();
            var sourceType = (RelocationSourceType)(sourceAndFlags & 0x0F);
            
            var sourceTypeString = sourceType switch
            {
                RelocationSourceType.LowByte => "LOBYTE",
                RelocationSourceType.Segment => "SEGMENT",
                RelocationSourceType.FarAddress => "FAR_ADDR",
                RelocationSourceType.Offset => "OFFSET",
                _ => $"UNKNOWN (0x{(byte)sourceType:X2})"
            };
            
            var flags = (RelocationFlags)(sourceAndFlags & 0xF0);
            
            var offset = reader.ReadUInt16();
            RelocationRecord record = new();
            SegmentRelocationModel model = new()
            {
                SegmentId = segmentId,
                RecordsCount = recordCount,
                SourceType = sourceTypeString,
                FixupType = "_",
                Ordinal = "_",
                Name = "_",
            };
            
            switch (flags & RelocationFlags.TargetMask)
            {
                case RelocationFlags.InternalRef:
                    var segmentType = reader.ReadByte();
                    var target = reader.ReadUInt16();
                    
                    model.RelocationType = "Internal Reference";
                    model.RelocationFlags.Add("REL_INTERNAL_REF");
                    model.Target = target;
                    // model.SegmentType = segmentType;
                    reader.ReadByte(); // Reserved (0)
                    
                    break;
                    
                case RelocationFlags.ImportName:
                    var modIndex = reader.ReadUInt16();
                    var modOffset = reader.ReadUInt16();
                    
                    var position = reader.BaseStream.Position; 
                    
                    reader.BaseStream.Position = Offset(NeHeader.NE_ImportModulesTable) + modOffset;
                    var length = reader.ReadByte();
                    
                    model.RelocationType = "Import by Name";
                    model.RelocationFlags.Add("REL_IMPORT_NAME");
                    model.Name = Encoding.ASCII.GetString(reader.ReadBytes(length)).TrimEnd('\0');
                    model.ModuleIndex = modIndex;
                    
                    reader.BaseStream.Position = position;
                    break;
                    
                case RelocationFlags.ImportOrdinal:
                    var modIndexOrd = reader.ReadUInt16();
                    var ordinal = reader.ReadUInt16();
                    
                    model.RelocationFlags.Add("REL_IMPORT_ORDINAL");
                    model.RelocationType = "Import Ordinal";
                    model.Ordinal = "@" + ordinal;
                    model.ModuleIndex = modIndexOrd;
                    
                    break;
                    
                case RelocationFlags.OSFixup:
                    var type = (OsFixupType)reader.ReadUInt16();
                    reader.ReadUInt16(); // Reserved (0)

                    model.RelocationType = "OS Fixup";
                    model.RelocationFlags.Add("REL_OSFIXUP");
                    model.RelocationFlags.Add(type.ToString());
                    model.FixupType = type.ToString();
                    
                    break;
                    
                default:
                    Debug.WriteLine($"Unknown relocation type: {flags & RelocationFlags.TargetMask}");
                    break;
            }
            
            record.SourceType = sourceType;
            record.Flags = flags;
            record.Offset = offset;
            result.Records.Add(record);
            
            relocationModel.Add(model);
        }
        
        return relocationModel;
    }
    private void FillNeSegmentModel(ref NeSegmentInfo segment, uint segmentNumber)
    {
        List<string> chars = [];

        if ((segment.Flags & (ushort)NeSegmentType.WithinRelocations) != 0) chars.Add("SEG_WITHIN_RELOCS");
        if ((segment.Flags & (ushort)NeSegmentType.Mask) != 0) chars.Add("SEG_HASMASK");
        if ((segment.Flags & (ushort)NeSegmentType.DiscardPriority) != 0) chars.Add("SEG_DISCARDABLE");
        if ((segment.Flags & (ushort)NeSegmentType.Movable) != 0) chars.Add("SEG_MOVABLE_BASE");
        if ((segment.Flags & (ushort)NeSegmentType.PreLoad) != 0) chars.Add("SEG_PRELOAD");
        
        var model = new NeSegmentModel(segment, segmentNumber, chars.ToArray());
        
        if (segment.FileLength == 0)
            model.FileLength = 0x10000;
        if (segment.FileOffset == 0)
            segment.Type = ".BSS"; // <-- those sectors/segments are extract while app is running.
        SegmentModels.Add(model);
    }

    private void FillModuleReferences(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(NeHeader.NE_ModReferencesTable);
        ModuleReferences = new NeModule[NeHeader.NE_ModReferencesCount];

        for (var i = 0; i < NeHeader.NE_ModReferencesCount; i++)
        {
            var mod = Fill<NeModule>(reader);
            ModuleReferences[i] = mod;
        }
    }

    /// <summary>
    /// Correct EntryTable processor
    /// </summary>
    /// <param name="reader"></param>
    /// <exception cref="InvalidDataException">If EntryTable not satisfied length itself</exception>
    private void FillEntryTable2(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(NeHeader.NE_EntryTable);
        
        var bundles = new List<NeEntryBundle>();
        int bytesRemaining = NeHeader.NE_EntriesCount;
        var currentOrdinal = 1;

        while (bytesRemaining > 0)
        {
            var count = reader.ReadByte();
            bytesRemaining--;

            if (count == 0) // <-- end
                break;

            var segId = reader.ReadByte();
            bytesRemaining--;

            var entries = new List<NeEntryTableModel>();

            if (segId == 0) // !UNuSED
            {
                for (var i = 0; i < count; i++)
                {
                    entries.Add(new NeEntryTableModel(true, false, 0)
                    {
                        Ordinal = (ushort)currentOrdinal++,
                        Segment = segId,
                    });
                }
            }
            else // CONST or MOVEABLE
            {
                var isMoveable = segId == 0xFF;
                var entrySize = isMoveable ? 6 : 3;
                var bundleDataSize = count * entrySize;

                if (bundleDataSize > bytesRemaining)
                    throw new InvalidDataException(
                        $"Inexact length: expected {bundleDataSize} bytes, got {bytesRemaining}");

                for (var i = 0; i < count; i++)
                {
                    if (isMoveable)
                    {
                        var flags = reader.ReadByte();
                        var magic = reader.ReadBytes(2);
                        var segment = reader.ReadByte();
                        var offset = reader.ReadUInt16();

                        entries.Add(new NeEntryTableModel(
                            false, true ,flags)
                        {
                            Offset = offset,
                            Segment = segment,
                            Ordinal = (ushort)currentOrdinal
                        });
                    }
                    else
                    {
                        var flags = reader.ReadByte();
                        var offset = reader.ReadUInt16();

                        entries.Add(new NeEntryTableModel(
                            false, false, flags)
                        {
                            Offset = offset,
                            Segment = segId,
                            Ordinal = (ushort)currentOrdinal,
                        });
                    }

                    currentOrdinal++;
                }

                bytesRemaining -= bundleDataSize;
            }

            bundles.Add(new NeEntryBundle()
            {
                EntryPoints = entries
            });
        }

        // iterate processed segmentEntries
        
        EntryTableItems = bundles;
    }
    
    /// <summary>
    /// Fills resident names
    /// </summary>
    /// <param name="reader"></param>
    private void FillResidentNames(BinaryReader reader)
    {
        List<NeExport> exports = [];

        reader.BaseStream.Position = Offset(NeHeader.NE_ResidentNamesTable);

        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            exports.Add(new NeExport
            {
                Count = i,
                Name = name,
                Ordinal = ordinal
            });
        }

        ResidentNames = exports;
    }
    
    /// <summary>
    /// Fills Not resident names
    /// </summary>
    /// <param name="reader"></param>
    private void FillNonResidentNames(BinaryReader reader)
    {
        List<NeExport> exports = [];

        if (NeHeader.NE_NonResidentNamesCount == 0)
            return;

        reader.BaseStream.Position = NeHeader.NE_NonResidentNamesTable;

        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            exports.Add(new NeExport
            {
                Count = i,
                Name = name,
                Ordinal = ordinal
            });
        }

        NonResidentNames = exports.ToArray();
    }

    /// <summary>
    /// Tries to fill suggesting imported module names and procedure names
    /// </summary>
    /// <param name="reader"></param>
    private void FillImports(BinaryReader reader)
    {
        List<NeImportModel> imports = new();
        var importTableOffset = Offset(NeHeader.NE_ImportModulesTable);

        reader.BaseStream.Position = Offset(NeHeader.NE_ModReferencesTable);
        var moduleRefOffsets = new ushort[NeHeader.NE_ModReferencesCount];
        for (var i = 0; i < NeHeader.NE_ModReferencesCount; i++)
        {
            moduleRefOffsets[i] = reader.ReadUInt16();
        }

        foreach (var moduleNameOffset in moduleRefOffsets)
        {
            reader.BaseStream.Position = importTableOffset + moduleNameOffset;
            NeImportModel moduleImport = new() { Functions = new() };
            
            // Module name check
            var nameLen = reader.ReadByte();
            moduleImport.DllName = Encoding.ASCII.GetString(reader.ReadBytes(nameLen));

            // Procedure name kek
            while (true)
            {
                var funcLen = reader.ReadByte();
                if (funcLen == 0) break;

                var isOrdinal = (funcLen & 0x80) != 0;
                var realLen = (byte)(funcLen & 0x7F);

                ImportingFunction func = new();

                if (isOrdinal) // <-- Module references are invalid. They took NonResidentNames table 
                {
                    var ordinal = reader.ReadUInt16();
                    func.Name = $"@{ordinal}";
                    func.Ordinal = ordinal;
                }
                else
                {
                    func.Name = Encoding.ASCII.GetString(reader.ReadBytes(realLen));
                    func.Ordinal = 0;
                }

                moduleImport.Functions.Add(func);
            }

            imports.Add(moduleImport);
        }

        ImportModels = imports.ToArray();
    }

    private void TryFindVb3RuntimeSegment(BinaryReader reader)
    {
        reader.BaseStream.Position = Segments[0].FileOffset;
        var endPosition = Segments[0].FileOffset + Segments[0].FileLength;
        var startPosition = reader.BaseStream.Position;

        
        
    }
}