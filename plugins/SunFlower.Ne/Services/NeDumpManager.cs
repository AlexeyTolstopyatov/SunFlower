using System.Diagnostics;
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

    private UInt32 _offset = 0;

    /// <returns> Raw file address </returns>
    private UInt32 Offset(UInt32 address)
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

        if (MzHeader.e_sign != 0x5a4d)
            throw new InvalidOperationException("Doesn't have DOS/2 signature");

        _offset = MzHeader.e_lfanew;
        stream.Position = _offset;

        NeHeader = Fill<NeHeader>(reader);

        if (NeHeader.magic != 0x454e)
            throw new InvalidOperationException("Doesn't have new signature");

        FillSegments(reader);
        FillModuleReferences(reader);
        FillEntries(reader);
        FillNonResidentNames(reader);
        FillImports(reader);
    }

    private void FillSegments(BinaryReader reader)
    {
        List<NeSegmentInfo> segTable = [];
        reader.BaseStream.Position = Offset(NeHeader.segtab);

        for (Int32 i = 0; i < NeHeader.cseg; i++)
        {
            NeSegmentInfo segment = Fill<NeSegmentInfo>(reader);
            FindSegmentCharacteristics(ref segment);

            segTable.Add(segment);
        }

        Segments = segTable.ToArray();
    }

    private void FindSegmentCharacteristics(ref NeSegmentInfo segment)
    {
        List<String> chars = [];

        if ((segment.Flags & (UInt16)NeSegmentType.WithinRelocations) != 0) chars.Add("SEG_WITHIN_RELOCS");
        if ((segment.Flags & (UInt16)NeSegmentType.Mask) != 0) chars.Add("SEG_HASMASK");
        if ((segment.Flags & (UInt16)NeSegmentType.DiscardPriority) != 0) chars.Add("SEG_DISCARDABLE");
        if ((segment.Flags & (UInt16)NeSegmentType.Movable) != 0) chars.Add("SEG_MOVABLE_BASE");
        if ((segment.Flags & (UInt16)NeSegmentType.Data) != 0) chars.Add("SEG_DATA");
        if ((segment.Flags & (UInt16)NeSegmentType.Code) != 0) chars.Add("SEG_CODE");

        SegmentModels.Add(new NeSegmentModel(segment, chars.ToArray()));
    }

    private void FillModuleReferences(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(NeHeader.modtab);
        ModuleReferences = new NeModule[NeHeader.cmod];

        for (Int32 i = 0; i < NeHeader.cmod; i++)
        {
            NeModule mod = Fill<NeModule>(reader);
            ModuleReferences[i] = mod;
        }
    }

    private void FillEntries(BinaryReader reader)
    {
        reader.BaseStream.Position = Offset(NeHeader.enttab);
        
        List<NeEntryTableModel> entries = [];
        long startPos = reader.BaseStream.Position;
        long endPos = startPos + NeHeader.cbenttab; // debug

        while (reader.BaseStream.Position < endPos)
        {
            byte entryCount = reader.ReadByte();
            if (entryCount == 0) break; // <-- entry count = 0;

            byte segmentIndicator = reader.ReadByte();

            for (int i = 0; i < entryCount; i++)
            {
                switch (segmentIndicator)
                {
                    case 0: // Unused entry
                        entries.Add(new NeEntryTableModel(isUnused: true, isMovable: false, 0));
                        break;

                    case < 0xFE: // Fixed segment
                        Byte flags = reader.ReadByte();
                        UInt16 offset = reader.ReadUInt16();
                    
                        entries.Add(new NeEntryTableModel(isUnused: false, isMovable: false, flags)
                        {
                            Segment = segmentIndicator,
                            Offset = offset
                        });
                        break;
                    case 0xFF: // Movable segment
                        Byte moveableFlags = reader.ReadByte();
                        Byte int3F = reader.ReadByte();
                        UInt32 addr = reader.ReadUInt16(); // must be 0x3F
                        Byte segment = reader.ReadByte();
                        UInt16 moveableOffset = reader.ReadUInt16();
    
                        // check
                        if(int3F != 0xCD && addr != 0x3F) // INT 
                            Debug.Print($"Invalid marker: 0x{int3F:X} 0x{addr:X}");
    
                        entries.Add(new NeEntryTableModel(isUnused: false, isMovable: true, moveableFlags)
                        {
                            Segment = segment,
                            Offset = moveableOffset,
                        });
                        break;
                }
            }
        }

        EntryTableItems = entries;
    }

    private void FillNonResidentNames(BinaryReader reader)
    {
        List<NeExport> exports = [];

        if (NeHeader.cbnrestab == 0)
            return;

        reader.BaseStream.Position = NeHeader.nrestab;

        Byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            String name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            UInt16 ordinal = reader.ReadUInt16();
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
        UInt32 importTableOffset = Offset(NeHeader.imptab);
        
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
                Byte realLength = (Byte)(funcLen & 0x7F);
                
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