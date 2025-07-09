using System.Data;
using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VddHeader
{
    [MarshalAs(UnmanagedType.U4)] 
    public uint LE_WindowsResInfoOffset;
    
    [MarshalAs(UnmanagedType.U4)] 
    public uint LE_WindowsResLength;
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_DeviceID;          // device ID (Windows VxD only).
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_DDKMajor;               // DDK version number.
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_DDKMinor;
    
}