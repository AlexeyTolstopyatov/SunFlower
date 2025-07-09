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
    public NeExport[] ExportingFunctions { get; set; } = [];
    public NeModule[] ModuleReferences { get; set; } = [];
    public List<NeSegmentModel> SegmentModels { get; set; } = [];
    public List<NeEntryTableModel> EntryTableItems { get; set; } = [];
    public List<uint> EntryPointsAddresses { get; set; } = [];

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

        if (NeHeader.magic != 0x454e && NeHeader.magic != 0x4e45) // magic or cigam
            throw new InvalidOperationException("Doesn't have new signature");

        FillSegments(reader);
        FillEntryTable(reader); // warn!
        FillModuleReferences(reader);
        FillNonResidentNames(reader);
        FillImports(reader);
        
        reader.Close();
    }

    private void FillSegments(BinaryReader reader)
    {
        List<NeSegmentInfo> segTable = [];
        reader.BaseStream.Position = Offset(NeHeader.segtab);

        for (int i = 0; i < NeHeader.cseg; i++)
        {
            NeSegmentInfo segment = Fill<NeSegmentInfo>(reader);
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
        reader.BaseStream.Position = Offset(NeHeader.modtab);
        ModuleReferences = new NeModule[NeHeader.cmod];

        for (int i = 0; i < NeHeader.cmod; i++)
        {
            NeModule mod = Fill<NeModule>(reader);
            ModuleReferences[i] = mod;
        }
    }

    private void FillEntryTable(BinaryReader reader)
    {
        List<NeEntryTableModel> entries = [];
        
        reader.BaseStream.Position = Offset(NeHeader.enttab);
        long endPosition = NeHeader.enttab + NeHeader.cbenttab;

        while (reader.BaseStream.Position < endPosition)
        {
            byte count = reader.ReadByte();
            if (count == 0) break;

            byte type = reader.ReadByte();

            if (type == 0xFF) // UNUSED
            {
                for (int i = 0; i < count; i++)
                {
                    entries.Add(new NeEntryTableModel(true, false, type)
                    {
                        Type = ".UNUSED",
                        Offset = 0
                    });
                    EntryPointsAddresses.Add(0); // null entry
                }

                continue;
            }

            for (int i = 0; i < count; i++)
            {
                ushort segId = 0;
                ushort offset;
                string bundleType;
                if (type == 0xFE) // MOVEABLE
                {
                    bundleType = ".MOVABLE";
                    byte segIndex = reader.ReadByte();
                    offset = reader.ReadUInt16();

                    if (segIndex > 0 && segIndex <= SegmentModels.Count)
                    {
                        segId = (UInt16)SegmentModels[segIndex - 1].SegmentId;
                    }
                }
                else // FIXED
                {
                    bundleType = ".FIXED";
                    offset = reader.ReadUInt16();

                    if (type > 0 && type <= SegmentModels.Count)
                    {
                        segId = (UInt16)SegmentModels[type - 1].SegmentId;
                    }
                }

                // Calculate address: segment * 0x1000 + offset
                uint address = segId > 0
                    ? (uint)(segId * 0x1000) + offset
                    : 0;

                EntryPointsAddresses.Add(address);
                entries.Add(new NeEntryTableModel(false, true, type)
                {
                    Type = bundleType,
                    Offset = offset,
                    Segment = segId,
                });
            }
        }

        EntryTableItems = entries;
    }
    public void CreateSymbols(List<uint> entryPoints, List<(int ordinal, string name)> nameTable)
    {
        foreach ((int ordinal, string name) entry in nameTable)
        {
            int ordinal = entry.ordinal;
            if (ordinal <= 0 || ordinal >= entryPoints.Count)
                continue;
            uint address = entryPoints[ordinal];
            if (address == 0)
                continue;
            string name = entry.name;
            
            // (name, address)
            NamesTable.Add(new ImportingFunction()
            {
                Name = name,
                Ordinal = (UInt16)ordinal,
                Offset = (UInt16)address
            });
        }
    }
    public void ProcessNameTable(BinaryReader reader, List<(int ordinal, string name)> table)
    {
        foreach (var entry in EntryTableItems)
        {
            int ordinal = entry.Segment; // Ordinal <-> Segment
            if (ordinal <= 0 || ordinal >= EntryPointsAddresses.Count) 
                continue;
            
            uint address = EntryPointsAddresses[ordinal];
            if (address == 0) 
                continue;
            
            // Create symbol: both named and @ordinal version
            //CreateSymbol(address, entry.Name);
            
            //CreateSymbol(address, $"@{ordinal}");
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
    
    private void FillNonResidentNames(BinaryReader reader)
    {
        List<NeExport> exports = [];

        if (NeHeader.cbnrestab == 0)
            return;

        reader.BaseStream.Position = NeHeader.nrestab;

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

        ExportingFunctions = exports.ToArray();
    }
    
    private void FillImports(BinaryReader reader)
    {
        List<NeImportModel> imports = new List<NeImportModel>();
        uint importTableOffset = Offset(NeHeader.imptab);
        
        foreach (NeModule neModule in ModuleReferences)
        {
            reader.BaseStream.Position = importTableOffset + neModule.ImportOffset;
            
            byte nameLen = reader.ReadByte();
            string moduleName = new(reader.ReadChars(nameLen));
            
            NeImportModel moduleImport = new()
            {
                DllName = moduleName,
                Functions = []
            };

            while (true)
            {
                byte funcLen = reader.ReadByte();
                if (funcLen == 0) break;
                
                bool isOrdinal = (funcLen & 0x80) != 0;
                byte realLength = (byte)(funcLen & 0x7F);
                
                if (isOrdinal)
                {
                    ushort ordinal = reader.ReadUInt16();
                    moduleImport.Functions.Add(new ImportingFunction
                    {
                        Name = $"@{ordinal}",
                        Ordinal = ordinal
                    });
                }
                else
                {
                    string funcName = Encoding.ASCII.GetString(reader.ReadBytes(realLength));
                    moduleImport.Functions.Add(new ImportingFunction
                    {
                        Name = funcName,
                        Ordinal = 0
                    });
                }
            }
            
            imports.Add(moduleImport);
        }
        
        ImportModels = imports.ToArray();
        ResolveImports();
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