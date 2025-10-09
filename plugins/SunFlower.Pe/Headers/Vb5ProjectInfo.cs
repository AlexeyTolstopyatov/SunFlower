using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vb5ProjectInfo
{
    /// <summary>
    /// Must contain 5.00 (0x1F4)
    /// </summary>
    public UInt32 Version;
    public UInt32 ObjectTablePointer;
    public UInt32 Null;
    /// <summary>
    ///  0xE9E9E9E9.
    /// </summary>
    public UInt32 CodeStartsPointer;
    /// <summary>
    ///  0x9E9E9E9E.
    /// </summary>
    public UInt32 CodeEndsPointer;
    public UInt32 DataSize;
    public UInt32 ThreadSpacePointer;
    public UInt32 VbaSEHPointer;
    /// <summary>
    /// If not zero -> application linked with Native x86 instructions
    /// elsewhere may contain Pseudo-Code instructions. 
    /// </summary>
    public UInt32 NativeCodePointer;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] 
    public Byte[] PrimitivePath; // Windows Unicode string 3 words
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 530)]
    public Byte[] ProjectPath;
    // &table -> [[],[],[],[]]
    public UInt32 ExternalTablePointer;
    public UInt32 ExternalTableCount;
}