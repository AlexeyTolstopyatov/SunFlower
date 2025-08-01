using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SunFlower.Ne.Headers;

// Type Vb3CodeSegment
//  sUnknown1 As String * 1
//  Token As String * 1
//  sizeOfUnknown__ As Integer
//  wUnknown__ As Integer
//  wUnknown__ As Integer
// End Type
/// <summary>
/// I've found this structure in DoDi VB Decompiler
/// And suggest, this data can be found in first .CODE segment of NE image
/// if sizeof(CODE) segment = sizeof(struct)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class Vb3SegmentHeader
{
    public char UnknownTarget1;
    public char Token;
    public ushort SizeOfUnknownTarget;
    public ushort UnknownTarget2;
    public ushort UnknownTarget3;
}
