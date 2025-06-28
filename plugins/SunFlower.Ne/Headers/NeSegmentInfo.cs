using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeSegmentInfo
{
    public UInt16 SegmentNumber;
    public UInt32 FileOffset;
    public UInt16 FileLength;
    public UInt16 Flags;
    public UInt16 MinAllocation;
    public String Type => (Flags & 0x0007) switch
    {
        0x0000 => "CODE16",
        0x0001 => "DATA16",
        _ => String.Empty
    };

    // [NonSerialized] // <-- I hope it will work. 
    // public String[] TranslatedFlags; // <-- this field fills manually
}