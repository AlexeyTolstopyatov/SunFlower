using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VxdHeader
{
    [MarshalAs(UnmanagedType.U4)] 
    public uint LE_WindowsResOffset;
    
    [MarshalAs(UnmanagedType.U4)] 
    public uint LE_WindowsResLength;
    
    [MarshalAs(UnmanagedType.U2)]
    public ushort LE_DeviceID;
    
    [MarshalAs(UnmanagedType.U1)]
    public byte LE_DDKMinor;
    [MarshalAs(UnmanagedType.U1)]
    public byte LE_DDKMajor;
}