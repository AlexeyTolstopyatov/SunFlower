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

        for (int i = 0; i < NeHeader.NE_SegmentsCount; i++)
        {
            NeSegmentInfo segment = Fill<NeSegmentInfo>(reader);
            segment.FileOffset *= alignment;
        
            FillNeSegmentModel(ref segment, (uint)i + 1);
            // pain...
            SegmentRelocations.AddRange(FillSegmentRelocations(reader, segment, i));
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
    public List<SegmentRelocationModel> FillSegmentRelocations(BinaryReader reader, NeSegmentInfo segment, int segmentId)
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
        uint relocationOffset = Convert.ToUInt32(segment.FileOffset + segment.FileLength);
        reader.BaseStream.Position = relocationOffset;
        
        // reading all records // out of bounds... but why
        ushort recordCount;
        try
        {
            recordCount = reader.ReadUInt16();

        }
        catch
        {
            recordCount = 0;
            return
            [
                new()
                {
                    RelocationFlags = ["REL_ERR"],
                    RelocationType = "Unable to read",
                    SegmentId = segmentId,
                    SegmentType = "SEG_FARADDR"
                }
            ];
        }
        
        for (int i = 0; i < recordCount; i++)
        {
            byte sourceAndFlags = reader.ReadByte();
            var sourceType = (RelocationSourceType)(sourceAndFlags & 0x0F);
            
            string sourceTypeString = sourceType switch
            {
                RelocationSourceType.LowByte => "LOBYTE",
                RelocationSourceType.Segment => "SEGMENT",
                RelocationSourceType.FarAddress => "FAR_ADDR",
                RelocationSourceType.Offset => "OFFSET",
                _ => $"UNKNOWN(0x{(byte)sourceType:X2})"
            };
            
            var flags = (RelocationFlags)(sourceAndFlags & 0xF0);
            
            ushort offset = reader.ReadUInt16();
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
                    byte segmentType = reader.ReadByte();
                    ushort target = reader.ReadUInt16();
                    
                    model.RelocationType = "Internal Reference";
                    model.RelocationFlags.Add("REL_INTERNAL_REF");
                    model.Target = 
                    reader.ReadByte(); // Reserved (0)
                    
                    break;
                    
                case RelocationFlags.ImportName:
                    ushort modIndex = reader.ReadUInt16();
                    ushort modOffset = reader.ReadUInt16();
                    
                    long position = reader.BaseStream.Position; 
                    
                    reader.BaseStream.Position = Offset(NeHeader.NE_ImportModulesTable) + modOffset;
                    byte length = reader.ReadByte();
                    
                    model.RelocationType = "Import by Name";
                    model.RelocationFlags.Add("REL_IMPORT_NAME");
                    model.Name = Encoding.ASCII.GetString(reader.ReadBytes(length));
                    model.ModuleIndex = modIndex;
                    
                    reader.BaseStream.Position = position;
                    break;
                    
                case RelocationFlags.ImportOrdinal:
                    ushort modIndexOrd = reader.ReadUInt16();
                    ushort ordinal = reader.ReadUInt16();
                    
                    model.RelocationFlags.Add("REL_IMPORT_ORDINAL");
                    model.RelocationType = "Import Ordinal";
                    model.Ordinal = "@" + ordinal;
                    model.ModuleIndex = modIndexOrd;
                    
                    break;
                    
                case RelocationFlags.OSFixup:
                    OsFixupType type = (OsFixupType)reader.ReadUInt16();
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
        if ((segment.Flags & (ushort)NeSegmentType.Data) != 0) chars.Add("SEG_DATA");
        if ((segment.Flags & (ushort)NeSegmentType.Code) != 0) chars.Add("SEG_CODE");

        SegmentModels.Add(new NeSegmentModel(segment, segmentId, chars.ToArray()));
    }

    private void FillModuleReferences(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(NeHeader.NE_ModReferencesTable);
        ModuleReferences = new NeModule[NeHeader.NE_ModReferencesCount];

        for (int i = 0; i < NeHeader.NE_ModReferencesCount; i++)
        {
            NeModule mod = Fill<NeModule>(reader);
            ModuleReferences[i] = mod;
        }
    }

    private void FillEntryTable(BinaryReader reader)
    {
        List<NeEntryTableModel> entries = new();
        EntryPointsAddresses = new();
        reader.BaseStream.Position = Offset(NeHeader.NE_EntryTable);
        
        ushort currentOrdinal = 1;
        long endPosition = reader.BaseStream.Position + NeHeader.NE_EntriesCount;

        while (reader.BaseStream.Position < endPosition)
        {
            byte entryCount = reader.ReadByte();
            if (entryCount == 0) break;

            byte segIndicator = reader.ReadByte();

            // warn: I've decided to skip unused ordinals.
            if (segIndicator == 0x00)
            {
                for (int i = 0; i < entryCount; i++)
                {
                    entries.Add(new NeEntryTableModel(true, false, segIndicator)
                    {
                        Type = "UNUSED",
                        Ordinal = currentOrdinal++
                    });
                    EntryPointsAddresses.Add(0);
                }
                continue;
            }

            for (int i = 0; i < entryCount; i++)
            {
                byte flags = 0;
                ushort offset = 0;
                byte segment = 0;
                string bundleType = "";

                switch (segIndicator)
                {
                    // .movable
                    case 0xFF:
                        bundleType = "MOVABLE";
                        flags = reader.ReadByte();
                        reader.ReadByte(); // INT 3FH (0x3F)
                        segment = reader.ReadByte();
                        offset = reader.ReadUInt16();
                        break;
                    // .fixed
                    case >= 0x01 and <= 0xFE:
                        bundleType = "FIXED";
                        flags = reader.ReadByte();
                        offset = reader.ReadUInt16();
                        segment = segIndicator;
                        break;
                }

                uint address = TryGetEntryPointPhysicalAddress(segment, offset);
                var entry = new NeEntryTableModel(false, true, segIndicator)
                {
                    Type = bundleType,
                    Segment = segment,
                    Offset = offset,
                    Flags = flags,
                    Ordinal = currentOrdinal
                };

                entries.Add(entry);
                EntryPointsAddresses.Add(address);
                currentOrdinal++;
            }
        }

        EntryTableItems = entries;
    }

    /// <summary>
    /// Tries to calculate EntryPoint physical file address
    /// if something went wrong - returns zero address.
    /// </summary>
    /// <param name="segmentId">Number of segment/object</param>
    /// <param name="offset"></param>
    /// <returns>Physical address of EntryPoint or zero</returns>
    private uint TryGetEntryPointPhysicalAddress(byte segmentId, ushort offset)
    {
        if (segmentId == 0) return 0;
        try
        {
            // Try to count Physical address using shifting /header set alignment/
            uint alignment = (uint)(1 << NeHeader.NE_Alignment); // Sector shift
            uint segmentAddress = Segments[segmentId - 1].FileOffset * alignment;
            
            return segmentAddress + offset;
        }
        catch
        {
            return 0;
        }
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
            string name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            ushort ordinal = reader.ReadUInt16();
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
            string name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            ushort ordinal = reader.ReadUInt16();
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
        uint importTableOffset = Offset(NeHeader.NE_ImportModulesTable);

        reader.BaseStream.Position = Offset(NeHeader.NE_ModReferencesTable);
        ushort[] moduleRefOffsets = new ushort[NeHeader.NE_ModReferencesCount];
        for (int i = 0; i < NeHeader.NE_ModReferencesCount; i++)
        {
            moduleRefOffsets[i] = reader.ReadUInt16();
        }

        foreach (ushort moduleNameOffset in moduleRefOffsets)
        {
            reader.BaseStream.Position = importTableOffset + moduleNameOffset;
            NeImportModel moduleImport = new() { Functions = new() };

            // Module name check
            byte nameLen = reader.ReadByte();
            moduleImport.DllName = Encoding.ASCII.GetString(reader.ReadBytes(nameLen));

            // Procedure name kek
            while (true)
            {
                byte funcLen = reader.ReadByte();
                if (funcLen == 0) break;

                bool isOrdinal = (funcLen & 0x80) != 0;
                byte realLen = (byte)(funcLen & 0x7F);

                ImportingFunction func = new();

                if (isOrdinal) // <-- Module references are invalid. They took NonResidentNames table 
                {
                    ushort ordinal = reader.ReadUInt16();
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