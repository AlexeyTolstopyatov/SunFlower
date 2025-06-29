using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeModule
{
    public UInt16 ImportOffset;
}