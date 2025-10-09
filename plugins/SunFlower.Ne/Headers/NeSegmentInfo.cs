using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeSegmentInfo
{
    public ushort FileOffset;
    public ushort FileLength;
    public ushort Flags;
    public ushort MinAllocation;
    private string _type;
    public string Type
    {
        get
        {
            _type = (Flags & 0x0007) switch
            {
                0x0001 => ".DATA",
                0x0002 => ".ITER",
                _ => ".CODE"
            };
            return _type;
        }
        set => _type = value;
    }
}