using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeSegmentInfo
{
    public ushort SegmentNumber;// Next fields by this are exists in docs
    public ushort FileOffset;
    public ushort FileLength;
    public ushort Flags;
    public ushort MinAllocation;
    public string Type => (Flags & 0x0007) switch
    {
        0x0000 => "CODE16",
        0x0001 => "DATA16",
        _ => string.Empty
    };
}