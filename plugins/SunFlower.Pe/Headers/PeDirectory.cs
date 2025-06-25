using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PeDirectory
{
    public UInt32 VirtualAddress;
    public UInt32 Size;
}