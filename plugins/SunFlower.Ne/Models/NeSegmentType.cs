namespace SunFlower.Ne.Models;
public enum NeSegmentType
{
    Code = 0x0000,
    Data = 0x0001,
    Mask = 0x0007,
    Movable = 0x0010,
    PreLoad = 0x0040,
    WithinRelocations = 0x0100,
    DiscardPriority = 0xF000
}