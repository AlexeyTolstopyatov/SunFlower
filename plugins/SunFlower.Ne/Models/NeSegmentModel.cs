using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Models;

public class NeSegmentModel(NeSegmentInfo info, string[] chars)
{
    public string Type { get; set; } = info.Type;
    
    public string[] Characteristics { get; set; } = chars;
    public UInt16 SegmentNumber { get; set; } = info.SegmentNumber;
    public UInt32 FileOffset { get; set; } = info.FileOffset;
    public UInt16 FileLength { get; set; } = info.FileLength;
    public UInt16 Flags { get; set; } = info.Flags;
    public UInt16 MinAllocation { get; set; } = info.MinAllocation;
}