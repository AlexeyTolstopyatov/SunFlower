namespace SunFlower.Ne.Models;

public class NeEntryTableModel(bool isUnused, bool isMovable, byte flags)
{
    public string Type { get; set; } = !isUnused
        ? isMovable 
            ? "Movable" 
            : "Fixed"
        : "Unused";
    public string Data { get; set; } = (flags & 0x02) != 0
        ? "Shared"
        : "Single";
    public string Entry { get; set; } = (flags & 0x01) != 0
        ? "Export"
        : "Static";

    public ushort Offset { get; set; }
    public ushort Segment { get; set; }
}