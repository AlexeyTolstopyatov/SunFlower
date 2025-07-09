using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Models;

public class NeSegmentModel(NeSegmentInfo info, uint segmentId, string[] chars)
{
    public string Type { get; set; } = info.Type;
    public string[] Characteristics { get; set; } = chars;
    public uint SegmentId { get; set; } = segmentId;
    public ushort SegmentNumber { get; set; } = info.SegmentNumber;
    public uint FileOffset { get; set; } = info.FileOffset;
    public ushort FileLength { get; set; } = info.FileLength;
    public ushort Flags { get; set; } = info.Flags;
    public ushort MinAllocation { get; set; } = info.MinAllocation;
}