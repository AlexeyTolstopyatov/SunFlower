namespace SunFlower.Le.Headers.Lx;

public enum EntryBundleType : byte
{
    Unused = 0,
    _16Bit = 1,
    _32Bit = 2,
    _286CallGate = 3,
    Forwarder = 4
}

public abstract class Entry
{
    public abstract EntryBundleType Type { get; }
}

public class Entry16Bit : Entry
{
    public override EntryBundleType Type { get; } = EntryBundleType._16Bit;
    public ushort ObjectNumber { get; set; }
    public byte Flags { get; set; }
    public ushort Offset { get; set; }
    public string EntryType => (Flags & 0x01) != 0 ? "[EXPORT]" : "[STATIC]";
    public string ObjectOffsets => (ObjectNumber == 0) ? "`absolute`" : "`virtual`";
}

public class Entry32Bit : Entry
{
    public override EntryBundleType Type { get; } = EntryBundleType._32Bit;
    public ushort ObjectNumber { get; set; }
    public byte Flags { get; set; }
    public uint Offset { get; set; }
    public string EntryType => (Flags & 0x01) != 0 ? "[EXPORT]" : "[STATIC]";
    public string ObjectOffsets => (ObjectNumber == 0) ? "`absolute`" : "`virtual`";
}
public class Entry286CallGate : Entry
{
    public override EntryBundleType Type => EntryBundleType._286CallGate;
    public ushort ObjectNumber { get; set; }
    public byte Flags { get; set; }
    public ushort Offset { get; set; }
    public ushort CallGateSelector { get; set; } // reserved. Fills by loader
    public string EntryType => (Flags & 0x01) != 0 ? "[EXPORT]" : "[STATIC]";
    public string ObjectOffsets => (ObjectNumber == 0) ? "`absolute`" : "`virtual`";
}

public class EntryForwarder : Entry
{
    public override EntryBundleType Type => EntryBundleType.Forwarder;
    public byte Flags { get; set; }
    public ushort ModuleOrdinal { get; set; }
    public uint OffsetOrOrdinal { get; set; }
    //public string EntryType => (Flags & 0x01) != 0 ? "[EXPORT]" : "[STATIC]";
    public string ObjectOffsets => "`virtual`";
}

public class EntryUnused : Entry
{
    public override EntryBundleType Type => EntryBundleType.Unused;
    public string ObjectOffsets => "`nope`";
    public string EntryType => "[SPACE]";
}

public class EntryBundle
{
    public byte Count { get; set; }
    public EntryBundleType Type { get; set; }
    public List<Entry> Entries { get; set; } = [];

    public string TypeString => Type switch
    {
        EntryBundleType._16Bit => "`.VALID_16`",
        EntryBundleType._32Bit => "`.VALID_32`",
        EntryBundleType._286CallGate => "`.CALLGATE`",
        EntryBundleType.Forwarder => "`.FWD`",
        EntryBundleType.Unused => "`.UNUSED`",
        _ => "`.WHAT?`"
    };

    public string TypeDescription => Type switch
    {
        EntryBundleType._16Bit => "Bundle contains records with `16-bit` offsets to exporting entries in program/library module.",
        EntryBundleType._32Bit => "Bundle contains records with `32-bit` offsets to exporting entries in program/library module.",
        EntryBundleType._286CallGate => "Bundle has entries which require execute in 2-ring (see Intel architecture). CallGate selector may be empty. It fills by `.EXE`/`.DLL` loader while app is running.",
        EntryBundleType.Forwarder => "Bundle has importing entries offsets to procedure name ASCII or import by ordinal. EntryTable may contains importing entries.",
        EntryBundleType.Unused => "Unused bundle not a runtime error. This is a space between exporting or importing entries for skipping enumeration. (@12, ..., @100).",
        _ => "If you see it - this bundle or all table entirely has a segmentation errors."
    };
}