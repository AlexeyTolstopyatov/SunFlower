namespace SunFlower.Ne.Models;

public class EntryBundle
{
    public int OrdinalBase { get; }
    public List<SegmentEntry> Entries { get; }

    public EntryBundle(int ordinalBase, List<SegmentEntry> entries)
    {
        OrdinalBase = ordinalBase;
        Entries = entries;
    }
}

public abstract class SegmentEntry { }

public class UnusedSegmentEntry : SegmentEntry { }

public class FixedSegmentEntry : SegmentEntry
{
    public byte Segment { get; }
    public byte Flags { get; }
    public ushort Offset { get; }

    public FixedSegmentEntry(byte segment, byte flags, ushort offset)
    {
        Segment = segment;
        Flags = flags;
        Offset = offset;
    }
}

public class MoveableSegmentEntry : SegmentEntry
{
    public byte Flags { get; }
    public byte[] Magic { get; } // |> INT 0x3F
    public byte Segment { get; }
    public ushort Offset { get; }

    public MoveableSegmentEntry(byte flags, byte[] magic, byte segment, ushort offset)
    {
        Flags = flags;
        Magic = magic;
        Segment = segment;
        Offset = offset;
    }
}