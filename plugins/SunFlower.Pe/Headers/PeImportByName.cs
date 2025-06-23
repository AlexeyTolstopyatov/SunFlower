using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential)]
public struct PeImportByName
{
    public UInt16 Hint;
    [MarshalAs(UnmanagedType.ByValArray)]
    public Char[] Name;
}