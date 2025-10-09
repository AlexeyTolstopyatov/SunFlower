namespace SunFlower.Ne.Headers;

public enum RelocationSourceType : byte
{
    LowByte = 0,        // LOBYTE
    Segment = 2,        // SEGMENT
    FarAddress = 3,     // FAR_ADDR (32-bit)
    Offset = 5,         // OFFSET (16-bit)
    Mask = 0x0F         // SOURCE_MASK
}

[Flags]
public enum RelocationFlags : byte
{
    InternalRef = 0,    // INTERNALREF
    ImportOrdinal = 1,  // IMPORTORDINAL
    ImportName = 2,     // IMPORTNAME
    OSFixup = 3,        // OSFIXUP
    Additive = 4,       // ADDITIVE
    TargetMask = 0x03   // TARGET_MASK
}

public enum OsFixupType : ushort
{
    FIARQQ_FJARQQ = 1,  // Floating-point
    FISRQQ_FJSRQQ = 2,
    FICRQQ_FJCRQQ = 3,
    FIERQQ = 4,
    FIDRQQ = 5,
    FIWRQQ = 6
}

public class RelocationRecord
{
    public RelocationSourceType SourceType { get; set; }
    public RelocationFlags Flags { get; }
    public ushort Offset { get; set; }
    public bool IsAdditive => (Flags & RelocationFlags.Additive) != 0;
}

public class InternalRefRelocation : RelocationRecord
{
    public byte SegmentType { get; } // 0xFF for movable
    public ushort Target { get; set; }    // Offset or ordinal
    
    public bool IsMovable => SegmentType == 0xFF;
    public string TargetType => IsMovable ? "MOVABLE" : "FIXED";
}

public class ImportNameRelocation : RelocationRecord
{
    public ushort ModuleIndex { get; set; }
    public ushort NameOffset { get; set; }
}

public class ImportOrdinalRelocation : RelocationRecord
{
    public ushort ModuleIndex { get; set; }
    public ushort Ordinal { get; set; }
}

public class OsFixupRelocation : RelocationRecord
{
    public OsFixupType FixupType { get; set; }
}

public class Relocation
{
    // for every record
    public uint SegmentNumber { get; set; }
    public byte AddressType { get; set; }
    public byte RelocationFlags { get; set; }
    public string RelocationType { get; set; } = string.Empty;
    public bool IsAdditive { get; set; }
    public ushort OffsetInSegment { get; set; }

    // internal reference
    public ushort SegmentType { get; set; }
    public ushort Target { get; set; }
    public string TargetType { get; set; } = string.Empty;

    // imports...
    public ushort ModuleIndex { get; set; }
    public ushort Ordinal { get; set; }
    public ushort NameOffset { get; set; }

    public string Fixup { get; set; } = string.Empty;
}