namespace SunFlower.Ne.Models;


public class EntryTableModel(bool isUnused, bool isMovable, byte flags)
{
    public string Type { get; set; } = !isUnused
        ? isMovable 
            ? "[MOVEABLE]" 
            : "[FIXED]"
        : "[UNUSED]";
    public string Data { get; set; } = (flags & 0x02) != 0
        ? "[Global](Shared)"
        : "[Single]";
    public string Entry { get; set; } = (flags & 0x01) != 0
        ? "Export"
        : "Static";
    public ushort Offset { get; set; }
    public ushort Segment { get; set; }
    public ushort Ordinal { get; set; }
    public byte Flags { get; set; } 
}

public class NeEntryBundle
{
    public List<EntryTableModel> EntryPoints { get; set; }
}