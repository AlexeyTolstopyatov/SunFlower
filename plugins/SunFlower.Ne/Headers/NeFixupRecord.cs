﻿namespace SunFlower.Ne.Headers;

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
    public RelocationFlags Flags { get; set; }
    public ushort Offset { get; set; }
    public bool IsAdditive => (Flags & RelocationFlags.Additive) != 0;
}

public class InternalRefRelocation : RelocationRecord
{
    public byte SegmentType { get; set; } // 0xFF for movable
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

public class SegmentRelocations
{
    public int SegmentId { get; set; }
    public List<RelocationRecord> Records { get; } = new();
    
    public IEnumerable<ImportNameRelocation> ImportNames => 
        Records.OfType<ImportNameRelocation>();
    
    public IEnumerable<ImportOrdinalRelocation> ImportOrdinals => 
        Records.OfType<ImportOrdinalRelocation>();
}