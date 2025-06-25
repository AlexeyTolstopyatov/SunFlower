using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct Cor20Header
{
    public UInt32 SizeOfHead;
    public UInt16 MajorRuntimeVersion;
    public UInt16 MinorRuntimeVersion;
    public UInt64 MetaDataOffset;
    public UInt32 LinkerFlags;
    public UInt32 EntryPointRva;
    public UInt32 EntryPointToken;

    public PeDirectory Resources;
    public PeDirectory StrongName;
    public PeDirectory CodeManager;
    public PeDirectory VTableDirectory;
    public PeDirectory Exports;
    public PeDirectory ManagedNativeHeader;
}