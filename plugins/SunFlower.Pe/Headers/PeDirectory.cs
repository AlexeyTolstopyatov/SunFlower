using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Explicit)]
public struct PeDirectory
{
    [FieldOffset(0x0)] public UInt32 VirtualAddress;
    [FieldOffset(0x4)] public UInt32 Size;
}