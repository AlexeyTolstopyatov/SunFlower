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
    public NeSegmentInfo[] Segments { get; set; } = [];
    public NeImportModel[] ImportModels { get; set; } = [];
    public NeExport[] NonResidentNames { get; set; } = [];
    public NeModule[] ModuleReferences { get; set; } = [];
    public List<NeSegmentModel> SegmentModels { get; set; } = [];
    public List<NeEntryTableModel> EntryTableItems { get; set; } = [];
    public List<uint> EntryPointsAddresses { get; set; } = [];
    public List<NeExport> ResidentNames { get; set; } = [];
    public List<SegmentRelocationModel> SegmentRelocations { get; set; } = [];

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
        FillEntryTable(reader); // warn!
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
    
        var alignment = (ushort)(1 << NeHeader.NE_Alignment); // means NE_SectorShift (0 is equivalent to 9?)

        for (var i = 0; i < NeHeader.NE_SegmentsCount; i++)
        {
            var segment = Fill<NeSegmentInfo>(reader);
            //segment.FileOffset *= alignment;
        
            FillNeSegmentModel(ref segment, (uint)i + 1);
            
            // pain...
            SegmentRelocations.AddRange(FillSegmentRelocations(reader, segment, i + 1));
            segTable.Add(segment);
        }

        Segments = segTable.ToArray();
    }
    /// <summary>
    /// REVIEW NEEDED. I give up to make tables. This entity must be checked
    ///
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
                new()
                {
                    RelocationFlags = ["REL_NO_RELOCS"],
                    RelocationType = "No relocations",
                    SegmentId = segmentId,
                    SegmentType = "SEG_WITHIN_RELOCS"
                }
            ];
        
        // try to calculate reloc
        Int64 relocationOffset = segment.FileOffset + segment.FileLength;
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
                new()
                {
                    RelocationFlags = ["REL_ERR"],
                    RelocationType = $"`Thrown at {segmentId}::{segment.Type}. (stopped at: {reader.BaseStream.Position:X})`",
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
                SourceType = sourceTypeString
            };
            
            switch (flags & RelocationFlags.TargetMask)
            {
                case RelocationFlags.InternalRef:
                    var segmentType = reader.ReadByte();
                    var target = reader.ReadUInt16();
                    
                    model.RelocationType = "Internal Reference";
                    model.RelocationFlags.Add("REL_INTERNAL_REF");
                    model.Target = 
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
                    model.Name = Encoding.ASCII.GetString(reader.ReadBytes(length));
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
    private void FillNeSegmentModel(ref NeSegmentInfo segment, uint segmentId)
    {
        List<string> chars = [];

        if ((segment.Flags & (ushort)NeSegmentType.WithinRelocations) != 0) chars.Add("SEG_WITHIN_RELOCS");
        if ((segment.Flags & (ushort)NeSegmentType.Mask) != 0) chars.Add("SEG_HASMASK");
        if ((segment.Flags & (ushort)NeSegmentType.DiscardPriority) != 0) chars.Add("SEG_DISCARDABLE");
        if ((segment.Flags & (ushort)NeSegmentType.Movable) != 0) chars.Add("SEG_MOVABLE_BASE");
        chars.Add((segment.Flags & 0x02) != 0 ? "SEG_ITERATED" : "SEG_NORMAL");
        chars.Add((segment.Flags & 0x01) == 0 ? "SEG_CODE" : "SEG_DATA");

        SegmentModels.Add(new NeSegmentModel(segment, segmentId, chars.ToArray()));
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

    private void FillEntryTable(BinaryReader reader) // <-- FIXES FIXES FIXES (see Win16ne)
    {
        List<NeEntryTableModel> entries = new();
        EntryPointsAddresses = new();
        reader.BaseStream.Position = Offset(NeHeader.NE_EntryTable);
        
        ushort currentOrdinal = 1;
        var endPosition = reader.BaseStream.Position + NeHeader.NE_EntriesCount;

        while (reader.BaseStream.Position < endPosition)
        {
            var entryCount = reader.ReadByte();
            
            if (entryCount == 0) break;

            var segIndicator = reader.ReadByte();

            // warn: I've decided to skip unused ordinals.
            if (segIndicator == 0)
            {
                for (var i = 0; i < entryCount; i++)
                {
                    entries.Add(new NeEntryTableModel(true, false, segIndicator)
                    {
                        Type = "[UNUSED]",
                        Ordinal = currentOrdinal++
                    });
                    EntryPointsAddresses.Add(0);
                }
                continue;
            }

            for (var i = 0; i < entryCount; i++)
            {
                byte flags = 0;
                ushort offset = 0;
                byte segment = 0;
                var bundleType = "";

                switch (segIndicator)
                {
                    // .fixed
                    case >= 0x01 and < 0xFE: // [0x01; 0xFE)
                        bundleType = $"[FIXED] 0x{segIndicator:X}";
                        flags = reader.ReadByte();
                        offset = reader.ReadUInt16();
                        segment = segIndicator;
                        break;
                    // .moveable
                    case 0xFF: // {0xFF}
                        bundleType = $"[MOVEABLE] 0x{segIndicator:X}";
                        flags = reader.ReadByte();
                        reader.ReadByte(); // INT 3FH (0x3F)
                        segment = reader.ReadByte();
                        offset = reader.ReadUInt16();
                        break;
                }

                var entry = new NeEntryTableModel(false, true, segIndicator)
                {
                    Type = bundleType,
                    Segment = segment,
                    Offset = offset,
                    Flags = flags,
                    Ordinal = currentOrdinal
                };

                entries.Add(entry);
                currentOrdinal++;
            }
        }

        EntryTableItems = entries;
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
            exports.Add(new NeExport()
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
            exports.Add(new NeExport()
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
        
}