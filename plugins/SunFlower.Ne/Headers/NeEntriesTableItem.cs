using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeEntriesTableItem
{
    public Byte EntriesCount;
    public Byte SegmentIndicator;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeEntriesTableFixedItem
{
    public Byte FlagWord;
    public UInt16 Offset;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NeEntriesTableMovableItem
{
    public Byte FlagWord;
    [MarshalAs(UnmanagedType.Struct)]
    public IntelInstruction Instruction;
    public Byte SegmentNumber;
    public UInt16 Offset;
}

public struct IntelInstruction
{
    public byte OpCode;
    public uint Address;
}