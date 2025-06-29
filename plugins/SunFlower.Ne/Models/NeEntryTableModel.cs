namespace SunFlower.Ne.Models;

public class NeEntryTableModel(bool isUnused, bool isMovable, Byte flags)
{
    public String Type { get; set; } = !isUnused
        ? isMovable 
            ? "Movable" 
            : "Fixed"
        : "Unused";
    public String Data { get; set; } = (flags & 0x02) != 0
        ? "Shared"
        : "Single";
    public String Entry { get; set; } = (flags & 0x01) != 0
        ? "Export"
        : "Static";

    public UInt16 Offset { get; set; }
    public UInt16 Segment { get; set; }
}