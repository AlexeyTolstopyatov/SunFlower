using System.Runtime.InteropServices;
namespace Sunflower.Mz.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MzRelocation
{
    public ushort Offset;
    public ushort Segment;
}