using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeSegmentInfo
{
    public ushort FileOffset;
    public ushort FileLength;
    public ushort Flags;
    public ushort MinAllocation;
    public string Type
    {
        get => (Flags & 0x0007) switch
        {
            0x0001 => ".DATA",
            0x0002 => ".ITER",
            _ => ".CODE"
        };
        set => throw new NotImplementedException();
    }
}

public struct SegmentRelocation
{
    public ushort OffsetInSegment { get; set; }
    public ushort Info { get; set; }
    public byte RelocationType => (byte)((Info >> 13) & 0x07); // 13-15
    public ushort Index => (ushort)(Info & 0x1FFF); // 0-12
}