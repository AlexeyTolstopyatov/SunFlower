using System.Runtime.InteropServices;

namespace SunFlower.Le.Headers.Le;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Object(uint virtualSegmentSize, uint relocationBase, uint objectFlagsMask, uint pageMap, uint pageMapEntries, uint unknown)
{
    public uint VirtualSegmentSize = virtualSegmentSize;
    public uint RelocationBaseAddress = relocationBase;
    public uint ObjectFlagsMask = objectFlagsMask;
    public uint PageMapIndex = pageMap;
    public uint PageMapEntries = pageMapEntries;
    public uint Unknown = unknown; // or Reserved
    public string[] ObjectFlags => new []
    {
        (ObjectFlagsMask & 0x1000) != 0 ? "16:16" : string.Empty, // both
        (ObjectFlagsMask & 0x2000) != 0 ? "USE_32" : "USE_16", // both
        (ObjectFlagsMask & 0x4000) != 0 ? "CONFORMING" : string.Empty, //  both
        (ObjectFlagsMask & 0x20000) != 0 ? "IO_PRIVILEGES" : string.Empty // both
        
    }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
    
    public bool Read => (ObjectFlagsMask & 0x0001) != 0;
    public bool Write => (ObjectFlagsMask & 0x0002) != 0;
    public bool Execute => (ObjectFlagsMask & 0x0004) != 0;
    public bool Resource => (ObjectFlagsMask & 0x0008) != 0;
    
    public string ObjectType => ((ObjectFlagsMask >> 8) & 0x03) switch
    {
        0 => "NORMAL", // <-- .CODE .DATA .BSS .RDATA .GOD (rwx object page)
        1 => "ZERO_FILLED",
        2 => "RESIDENT",
        3 => "RESIDENT_CONTIGUOUS",
        _ => "UNKNOWN"
    };
    /// <summary>
    /// Tries to tell Borland-declared name of object section.
    /// </summary>
    public static string GetSuggestedNameByPermissions(Object obj)
    {
        var r = obj.Read;
        var w = obj.Write;
        var x = obj.Execute;
        var v = obj.VirtualSegmentSize;

        return r switch
        {
            true when w && x => ".GOD", // rwx sections may be a signs of malware
            true when x => ".CODE",
            true when w => ".DATA",
            true when v == 0 => ".BSS",
            _ => ".OBJECT"
        };
    }
}