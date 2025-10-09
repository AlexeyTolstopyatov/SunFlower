namespace SunFlower.Ne.Headers;

public class Import
{
    public string Module { get; set; } = string.Empty;
    public string Procedure { get; set; } = string.Empty;
    public string Ordinal { get; set; } = string.Empty;
    public long NameOffset { get; set; }
    public long ModuleIndex { get; set; }
    public long OffsetInSegment { get; set; }
    public long SegmentNumber { get; set; }
}