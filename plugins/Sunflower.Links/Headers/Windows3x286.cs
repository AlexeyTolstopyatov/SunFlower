using System.Runtime.InteropServices;

namespace Sunflower.Links.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Windows3x286
{
    public ushort XmsMemMaxSizeK;
    public ushort XmsMemReqSizeK;
    public ushort Flags;
}