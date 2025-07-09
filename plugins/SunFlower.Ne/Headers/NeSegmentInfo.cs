using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeSegmentInfo
{
    public UInt16 SegmentNumber;// Next fields by this are exists in docs
    public UInt32 FileOffset;
    public UInt16 FileLength;
    public UInt16 Flags;
    public UInt16 MinAllocation;
    public string Type => (Flags & 0x0007) switch
    {
        0x0000 => "CODE16",
        0x0001 => "DATA16",
        _ => string.Empty
    };
}