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
    public NeImport[] ImportingModules { get; set; } = [];
    public NeImport[] ImportingFunctions { get; set; } = [];
    public NeExport[] ExportingFunctions { get; set; } = [];
    public NeModule[] ModuleReferences { get; set; } = [];
    public List<NeSegmentModel> SegmentModels { get; set; } = [];

    public List<NeEntriesTableFixedItem> FixedItems { get; set; } = [];
    public List<NeEntriesTableMovableItem> MovableItems { get; set; } = [];
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
        List<NeEntryTableModel> entries = [];
        List<NeEntriesTableItem> items = [];
        List<NeEntriesTableFixedItem> fixedItems = [];
        List<NeEntriesTableMovableItem> movableItems = [];

        // Seek Items in EntryTable
        for (Int32 i = 0; i < NeHeader.cbenttab; i++)
        {
            NeEntriesTableItem item = Fill<NeEntriesTableItem>(reader);

            items.Add(item);
            switch (item.SegmentIndicator)
            {
                case 0:
                    break;
                case > 0x001 and <= 0x0FE:
                    NeEntriesTableFixedItem fitem = Fill<NeEntriesTableFixedItem>(reader);
                    fixedItems.Add(fitem);
                    entries.Add(new(false, fitem.FlagWord)
                    {
                        Offset = fitem.Offset,
                        Segment = 0
                    });
                    break;
                default:
                    NeEntriesTableMovableItem mitem = Fill<NeEntriesTableMovableItem>(reader);
                    movableItems.Add(mitem);
                    entries.Add(new(true, mitem.FlagWord)
                    {
                        Offset = mitem.Offset,
                        Segment = mitem.SegmentNumber,
                        //MovableInstruction = mitem.Instruction
                    });
                    break;
            }
        }

        EntryTableItems = entries;
    }

    private void FillNonResidentNames(BinaryReader reader)
    {
        List<NeExport> exports = new();

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
    }

    private void FillImportingModules(BinaryReader reader)
    {
        List<NeImport> imports = [];
        foreach (NeModule neModule in ModuleReferences)
        {
            reader.BaseStream.Position = Offset(NeHeader.imptab) + neModule.ImportOffset;
            Byte nameLength = reader.ReadByte();
            String moduleName = new(reader.ReadChars(nameLength));

            NeImport module = new()
            {
                Name = moduleName,
                NameLength = nameLength
            };
            
            imports.Add(module);
        }

        ImportingModules = imports.ToArray();
    }
}