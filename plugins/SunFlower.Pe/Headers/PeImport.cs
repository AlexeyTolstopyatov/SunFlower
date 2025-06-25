using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

public struct PeImport
{
    [MarshalAs(UnmanagedType.ByValArray)]
    public Char[] Name;
    [MarshalAs(UnmanagedType.Struct)]
    public PeImportDescriptor Descriptor;
}