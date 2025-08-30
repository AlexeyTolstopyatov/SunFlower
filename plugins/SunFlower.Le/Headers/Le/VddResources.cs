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

public enum Win32ResourceType
{
    VS_VERSION_INFO = 0,
    StringFileInfo = 1,
    VarFileInfo = 2,
    StringTable = 3,
    String = 4,
    Var = 5
}
public class Win32Resource
{
    public ushort Length { get; set; }
    public ushort ValueLength { get; set; }
    public ushort Type { get; set; }
    public string Key { get; set; } = string.Empty;
    public byte[] Value { get; set; } = [];
    public int BlockType { get; set; }
    public List<Win32Resource> Children { get; set; } = [];
}
public class StringTable
{
    public string LanguageCode { get; set; }
    public Dictionary<string, string> Strings { get; set; } = new();
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