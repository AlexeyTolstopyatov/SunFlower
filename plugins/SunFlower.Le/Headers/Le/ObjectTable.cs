using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ObjectTable
{
    public uint VirtualSegmentSize;
    public uint RelocationBaseAddress;
    public uint ObjectFlags;
    public uint PageMapIndex;
    public uint PageMapEntries;
    public uint Unknown; // or Reserved
}