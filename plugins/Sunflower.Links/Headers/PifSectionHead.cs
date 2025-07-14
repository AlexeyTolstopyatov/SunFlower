using System.Runtime.InteropServices;

namespace Sunflower.Links.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
public struct PifSectionHead
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public char[] Name;

    public ushort NextSectionOffset;
    public ushort PartitionOffset;
    public ushort DataLength;
}