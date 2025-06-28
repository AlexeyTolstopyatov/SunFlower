using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeImport
{
    public UInt16 NameLength;
    public String Name;
}