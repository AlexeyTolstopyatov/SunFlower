namespace SunFlower.Pe.Headers;

public struct PeImportDescriptor64
{
    public UInt64 OriginalFirstThunk;
    public UInt32 TimeDateStamp;
    public UInt32 ForwarderChain;
    public UInt64 Name;
    public UInt64 FirstThunk;
}