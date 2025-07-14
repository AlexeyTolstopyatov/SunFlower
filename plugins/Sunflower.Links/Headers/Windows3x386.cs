using System.Numerics;
using System.Runtime.InteropServices;

namespace Sunflower.Links.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Windows3x386
{
    public ushort ConventionalMemMaxSizeK;
    public ushort ConventionalMemReqSizeK;
    public ushort ActivePriority;
    public ushort BackgroundPriority;
    public ushort EmsMemMaxSizeK;
    public ushort EmsMemReqSizeK;
    public ushort XmsMemMaxSizeK;
    public ushort XmsMemReqSizeK;
    public uint DosModeFlags; // I really don't know how to name it.
    public ushort VideoFlags;
    public ushort Reserved1;
    public ushort ShortCutKeyCode;
    public ushort ShortCutKeyModifierFlag;
    public ushort Reserved2;
    public ushort Reserved3;
    public uint Reserved4;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ArgumentsVector;
}