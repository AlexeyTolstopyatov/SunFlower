namespace SunFlower.Pe.Headers;

public struct PeImportDescriptor
{
    public UInt32 OriginalFirstThunk;
    public UInt32 TimeDateStamp;
    public UInt32 ForwarderChain;
    public UInt32 Name;
    public UInt32 FirstThunk;
}