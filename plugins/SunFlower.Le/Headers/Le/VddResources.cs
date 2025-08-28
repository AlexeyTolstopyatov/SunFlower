using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Microsoft.FSharp.Core;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FixedFileInfo 
{
    public int Signature;
    public int StrucVersion;
    public int FileVersionMs;
    public int FileVersionLs;
    public int ProductVersionMs;
    public int ProductVersionLs;
    public int FileFlagsMask;
    public int FileFlags; // Debug build, patched build, pre-release, private build or special build
    public int FileOs; // means Win16, Win32, WinNT/2000 or Win32s 
    public int FileType; // Executable, DLL, device driver, font, VXD or static library
    public int FileSubtype;
    public int FileDateMs;
    public int FileDateLs;
}
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct VersionInfo
{
    public ushort Length;
    public ushort ValueLength;
    public ushort Type;
    public string Key; // "VS_VERSION_INFO"
    // Alignment till 32-bit bound
    public FixedFileInfo Value;
    // Children goes here
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VddResources
{
    public byte Type;
    public ushort Id;
    public byte Name;
    public ushort Ordinal;
    public ushort Flags;
    public uint ResourceSize;
    //[MarshalAs(UnmanagedType.Struct)]
    //public VersionInfo VersionInfo;
}