using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ObjectPage
{
    public ushort HighPage; //
    public byte LowPage;    //
    public byte Flags;      // :D!!!
    [Flags]
    public enum PageFlags : byte
    {
        // Page types
        TypeMask        = 0b00000011,
        Legal           = 0b00000000,
        Iterated        = 0b00000001,
        Invalid         = 0b00000010,
        ZeroFilled      = 0b00000011,
        
        // status (2-6 unknown)
        
        LastPageInFile  = 0b10000000
    }
}