using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeResource
{
    public UInt32 ShiftsCount;
    [MarshalAs(UnmanagedType.Struct)]
    public NeResourceInfoHeader InfoHeader;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeResourceInfoHeader
{
    public UInt32 TypeId;
    public UInt32 ResourcesCount;
    public UInt32 Reserved;
    [MarshalAs(UnmanagedType.Struct)]
    public NeResourceTypeHeader TypeHeader;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeResourceTypeHeader
{
    public UInt32 ContentOffset;
    public UInt32 Size;
    public UInt32 Flag;
}