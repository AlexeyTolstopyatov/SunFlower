using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PeSection 
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public Char[] Name;
    public UInt32 VirtualSize;
    public UInt32 VirtualAddress;
    public UInt32 SizeOfRawData;
    public UInt32 PointerToRawData;
    public UInt32 PointerToRelocations;
    public UInt32 PointerToLinenumbers;
    public UInt16 NumberOfRelocations;
    public UInt16 NumberOfLinenumbers;
    public UInt32 Characteristics;
}