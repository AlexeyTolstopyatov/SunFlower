using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;

namespace SunFlower.Ne.Services;

public class NeSegmentTableManager
{
    public List<SegmentModel> SegmentModels { get; } = [];
    // Segments and segment relocations fills correctly and are not NULL
    // redundant warning
    
    #pragma warning disable
    public NeSegmentTableManager(BinaryReader reader, uint segtab, uint segcount, uint alignment)
    {
        FillSegments(reader, segtab, segcount, alignment);
    }
    #pragma warning restore
    
    private void FillSegments(BinaryReader reader, uint segtab, uint segcount, uint alignment)
    {
        List<NeSegmentInfo> segTable = [];
        reader.BaseStream.Position = segtab;
        
        for (var i = 0; i < segcount; i++)
        {
            var segment = new NeSegmentInfo
            {
                FileOffset = reader.ReadUInt16(), // shifted
                FileLength = reader.ReadUInt16(),
                Flags = reader.ReadUInt16(),
                MinAllocation = reader.ReadUInt16()
            };
            
            segTable.Add(segment);
            FillNeSegmentModel(ref segment, (uint)i + 1);
        }
        
        // exclude all segment records which flags not have 0x0100 
        foreach (var segment in SegmentModels.Where(segment => (segment.Flags & 0x0100) != 0))
        {
            if (alignment == 0)
                alignment = 9; // <-- 2^9 = 512 (paragraph allocation)

            var sectorShift = 1 << (int)alignment;
            
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
                            TargetType = segmentType == 0xFF ? "FIXED" : "MOVABLE",
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
                        reader.ReadUInt16(); // reserved WORD skip
                        
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
            segment.Relocations = segmentRel; // problem???
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
        
        // 0x0100 byte mask -> relocations exists
        if ((segment.Flags & 0x0100) == 0) chars.Add("WITHIN_RELOCS");
        if ((segment.Flags & (ushort)SegmentType.Mask) != 0) chars.Add("HAS_MASK");
        if ((segment.Flags & (ushort)SegmentType.DiscardPriority) != 0) chars.Add("DISCARDABLE");
        if ((segment.Flags & (ushort)SegmentType.Movable) != 0) chars.Add("MOVABLE_BASE");
        if ((segment.Flags & (ushort)SegmentType.PreLoad) != 0) chars.Add("PRELOAD");
        
        var model = new SegmentModel(segment, segmentNumber, chars.ToArray());
        
        if (segment.FileLength == 0)
            model.FileLength = 0x10000;
        if (segment.FileOffset == 0)
            segment.Type = ".BSS"; // <-- those sectors/segments are extract while app is running.
        SegmentModels.Add(model);
    }
}