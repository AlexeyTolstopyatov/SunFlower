using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Text;
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
    public List<ImportingFunction> NamesTable { get; set; } = [];

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
    
        var alignment = (uint)(1 << NeHeader.NE_Alignment); // means NE_SectorShift (0 is equivalent to 9?)

        for (int i = 0; i < NeHeader.NE_SegmentsCount; i++)
        {
            NeSegmentInfo segment = Fill<NeSegmentInfo>(reader);
            segment.FileOffset *= alignment;
        
            FillNeSegmentModel(ref segment, (uint)i + 1);
            segTable.Add(segment);
        }

        Segments = segTable.ToArray();
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

                uint address = CalculateSegmentAddress(segment, offset);
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

    private uint CalculateSegmentAddress(byte segmentId, ushort offset)
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
    private List<uint> ConvertToAddresses(List<NeEntryTableModel> entryPoints)
    {
        const ushort segmentBase = 0x1000;  // NULL page avoid
        List<uint> addresses = [];
    
        foreach (NeEntryTableModel ep in entryPoints) {
            // Address = (SegmentID * 0x1000) + offset
            uint address = (uint)(ep.Segment * segmentBase) + ep.Offset;
            addresses.Add(address);
        }
    
        return addresses;
    }

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
            
                if (isOrdinal)
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
    private void ResolveImports()
    {
        foreach (NeImportModel module in ImportModels)
        {
            foreach (ImportingFunction func in module.Functions.Where(f => f.Ordinal > 0))
            {
                int entryIndex = func.Ordinal - 1; // @ordinals starts from 1 _(not 0)
            
                if (entryIndex < EntryTableItems.Count)
                {
                    NeEntryTableModel entry = EntryTableItems[entryIndex];
                    func.Segment = entry.Segment;
                    func.Offset = entry.Offset;
                }
            }
        }
    }
}