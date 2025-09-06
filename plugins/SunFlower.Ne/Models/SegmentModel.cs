using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Models;

public class SegmentModel(NeSegmentInfo info, uint segmentNumber, string[] chars)
{
    public string Type { get; set; } = info.Type;
    public string[] Characteristics { get; set; } = chars;
    public uint SegmentNumber { get; set; } = segmentNumber;
    public uint FileOffset { get; set; } = info.FileOffset;
    public uint FileLength { get; set; } = info.FileLength;
    public ushort Flags { get; set; } = info.Flags;
    public ushort MinAllocation { get; set; } = info.MinAllocation;
    public List<Relocation> Relocations { get; set; } = [];
}