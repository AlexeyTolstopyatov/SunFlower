using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Models;

public class SegmentModel(NeSegmentInfo info, uint segmentNumber, string[] chars)
{
    public string Type { get; } = info.Type;
    public string[] Characteristics { get; } = chars;
    public uint SegmentNumber { get; } = segmentNumber;
    public uint FileOffset { get; } = info.FileOffset;
    public uint FileLength { get; set; } = info.FileLength;
    public ushort Flags { get; } = info.Flags;
    public ushort MinAllocation { get; } = info.MinAllocation;
    public List<Relocation> Relocations { get; set; } = [];
}