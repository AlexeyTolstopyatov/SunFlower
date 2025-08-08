using System.Security.AccessControl;

namespace SunFlower.Le.Headers.Le;

/// <summary>
/// EntryTables looks like collection of bundles which
/// generalize or group entries by common characteristics.
///
/// |---------------------|
/// | only 32-bit entries |
/// | only UNUSED 32-bit  |
/// | only 16-bit entries |
/// | ...                 |
/// 
/// </summary>
/// <param name="count"></param>
/// <param name="entIndex"></param>
/// <param name="objIndex"></param>
public class EntryBundle(byte count, byte entIndex, ushort objIndex)
{
    public byte EntriesCount { get; set; } = count;
    public byte EntryBundleIndex { get; set; } = entIndex;
    public string EntriesUsage => (EntryBundleIndex & 0b00000001) != 0 ? ".VALID" : ".UNUSED";
    public string EntriesSafeSize => (EntryBundleIndex & 0b00000010) != 0 ? "32-bit" : "16-bit";
    public ushort ObjectIndex { get; set; } = objIndex;
    /// <summary>
    /// Entry Bundle depends on EntryBundle Header byte mask
    /// must have 16-bit XOR 32-bit entries.
    /// </summary>
    public Entry[] Entries { get; set; } = [];
}

/// <summary>
/// Safe size will be different depends on <see cref="EntryBundle"/>
/// definition byte-mask. (32 or 16 offsets)
/// </summary>
public class Entry(int ordinal, byte flag, uint offset)
{
    public int Ordinal { get; } = ordinal;
    public byte Flag { get; init; } = flag;
    public uint Offset { get; init; } = offset;
    public string EntryType => (Flag & 0x01) != 0 ? "[EXPORT]" : "[STATIC]";
    public string ObjectType => (Flag & 0x02) != 0 ? "[SHARED]" : "[IMPURE]";
}
