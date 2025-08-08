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
            throw new InvalidOperationException("Doesn't have NE signature");

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
        
        foreach (var segment in SegmentModels)
        {
            if ((segment.Flags & 0x0C) == 0) 
                continue; // within relocs
            
            var alignment = NeHeader.NE_Alignment;
            if (alignment == 0)
                alignment = 9; // <-- 2^9 = 512 (paragraph allocation)

            var sectorShift = 1 << alignment;
            
            // physical offset allocation
            var segmentDataOffset = segment.FileOffset * sectorShift;
            var segmentDataLength = segment.FileLength == 0 ? 0x10000 : segment.FileLength;
            
            var relocationTableOffset = segmentDataOffset + segmentDataLength;
            
            if (relocationTableOffset + 2 > reader.BaseStream.Length) 
                continue;

            reader.BaseStream.Seek(relocationTableOffset, SeekOrigin.Begin);
            List<Relocation> segmentRel = [];
            
            // start the per-segment records
            var relocationCount = reader.ReadUInt16();

            for (var j = 0; j < relocationCount; j++)
            {
                var atp = reader.ReadByte();
                var relFlags = reader.ReadByte();
                var rtp = relFlags & 0x03;
                var isAdditive = (relFlags & 0x04) != 0;
                var offsetInSegment = reader.ReadUInt16();
                
                switch ((RelocationFlags)rtp)
                {
                    case RelocationFlags.InternalRef:
                        var segmentType = reader.ReadByte();
                        var target = reader.ReadUInt16();
                        
                        reader.ReadByte(); // Reserved (0)
                        segmentRel.Add(new Relocation
                        {
                            OffsetInSegment = offsetInSegment,
                            IsAdditive = isAdditive,
                            RelocationFlags = relFlags,
                            RelocationType = "Internal",
                            AddressType = atp,
                            SegmentType = segmentType,
                            TargetType = (segmentType == 0xFF) ? "FIXED" : "MOVABLE",
                            Target = target
                        });
                        break;
                    case RelocationFlags.ImportOrdinal:
                        var moduleIndex = reader.ReadUInt16();
                        var procOrdinal = reader.ReadUInt16();
                        
                        segmentRel.Add(new Relocation
                        {
                            OffsetInSegment = offsetInSegment,
                            IsAdditive = isAdditive,
                            AddressType = atp,
                            RelocationFlags = relFlags,
                            RelocationType = "Import",
                            Ordinal = procOrdinal,
                            ModuleIndex = moduleIndex
                        });
                        break;
                    case RelocationFlags.ImportName:
                        var moduleIndex2 = reader.ReadUInt16();
                        var procNameOffset = reader.ReadUInt16();
                        // try to get name???
                        
                        segmentRel.Add(new Relocation
                        {
                            OffsetInSegment = offsetInSegment,
                            IsAdditive = isAdditive,
                            AddressType = atp,
                            RelocationFlags = relFlags,
                            RelocationType = "Import",
                            ModuleIndex = moduleIndex2,
                            NameOffset = procNameOffset,
                        });
                        
                        break;
                    case RelocationFlags.OSFixup:
                        var osFixup = (OsFixupType)reader.ReadUInt16();
                        var reservedWord = reader.ReadUInt16();
                        
                        segmentRel.Add(new Relocation
                        {
                            OffsetInSegment = offsetInSegment,
                            IsAdditive = isAdditive,
                            AddressType = atp,
                            RelocationFlags = relFlags,
                            RelocationType = "OS Fixup",
                            Fixup = osFixup.ToString()
                        });
                        
                        break;
                }
            }
            segment.Relocations = segmentRel; // problem
        }
    }
    /// <summary>
    /// Fills and Pushes prepared model of segment entry in global SegmentsTable
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="segmentNumber"></param>
    private void FillNeSegmentModel(ref NeSegmentInfo segment, uint segmentNumber)
    {
        List<string> chars = [];

        if ((segment.Flags & 0x0C) == 0) chars.Add("SEG_WITHIN_RELOCS");
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
    private void FillEntryTable(BinaryReader reader)
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

        // erase dll names from every Importing Function
        var dllNames = imports.Select((i => i.DllName)).ToList();
        var erasedImportModels = new List<NeImportModel>();
        
        foreach (var import in imports)
        {
            var erased = import.Functions.Where(i => !dllNames.Contains(i.Name)).ToList();
            erasedImportModels.Add(new NeImportModel()
            {
                Functions = erased,
                DllName = import.DllName
            });
        }
        ImportModels = erasedImportModels.ToArray();
    }
    
    private void TryFindVb3RuntimeSegment(BinaryReader reader)
    {
        reader.BaseStream.Position = Segments[0].FileOffset;
        var endPosition = Segments[0].FileOffset + Segments[0].FileLength;
        var startPosition = reader.BaseStream.Position;

        
        
    }
}