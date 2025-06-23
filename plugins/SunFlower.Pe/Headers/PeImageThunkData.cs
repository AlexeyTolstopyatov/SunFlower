namespace SunFlower.Pe.Headers;

public struct PeImageThunkData
{
    public UInt32 Function;             // address of imported function
    public UInt32 Ordinal;              // ordinal value of function
    public UInt32 AddressOfData;        // RVA of imported name
    public UInt32 ForwarderString;      // RVA to forwarder string
}