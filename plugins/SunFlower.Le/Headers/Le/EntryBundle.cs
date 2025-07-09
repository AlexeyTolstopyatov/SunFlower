namespace SunFlower.Le.Headers.Le;

public class EntryBundle
{
    public byte EntriesCount { get; set; }
    public byte EntryBundleIndex { get; set; }
    public ushort ObjectIndex { get;set; }
    public Entry16[] Entries { get; set; } = [];
    public Entry32[] ExtendedEntries { get; set; } = [];
}

public class Entry32
{
    public byte Flag { get; set; }
    public uint Offset { get; set; }
    public string[] FlagNames { get; set; } = [];
}

public class Entry16
{
    public byte Flag { get; set; }
    public ushort Offset { get; set; }
    public string[] FlagNames { get; set; } = [];
}
