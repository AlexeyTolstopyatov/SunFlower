namespace SunFlower.Ne.Models;

public class EntryBundle
{
    public int OrdinalBase { get; }
    public List<Entry> Entries { get; }

    public EntryBundle(int ordinalBase, List<Entry> entries)
    {
        OrdinalBase = ordinalBase;
        Entries = entries;
    }
}

public abstract class Entry { }

public class UnusedEntry : Entry { }

public class FixedEntry : Entry
{
    public byte Segment { get; }
    public byte Flags { get; }
    public ushort Offset { get; }

    public FixedEntry(byte segment, byte flags, ushort offset)
    {
        Segment = segment;
        Flags = flags;
        Offset = offset;
    }
}

public class MoveableEntry : Entry
{
    public byte Flags { get; }
    public byte[] Magic { get; } // |> INT 0x3F
    public byte Segment { get; }
    public ushort Offset { get; }

    public MoveableEntry(byte flags, byte[] magic, byte segment, ushort offset)
    {
        Flags = flags;
        Magic = magic;
        Segment = segment;
        Offset = offset;
    }
}