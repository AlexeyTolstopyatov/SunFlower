namespace SunFlower.Le.Headers.Le;

public class FixupRecord
{
    public byte AddressType { get; set; } // ATP (Address Type and Flags)
    public byte RelocationType { get; set; } // RTP (Relocation Type and Flags)
    public ushort[] Offsets { get; set; } = [];
    public byte? TargetObject { get; set; } // Inner references
    public byte? ModuleIndex { get; set; } // Import records
    public ushort? Ordinal { get; set; } // Imports by Ordinal
    public ushort? NameOffset { get; set; } // Imports by Name
    public int? AddValue { get; set; } // Additive value
    public ushort? ExtraData { get; set; } // Extra
}