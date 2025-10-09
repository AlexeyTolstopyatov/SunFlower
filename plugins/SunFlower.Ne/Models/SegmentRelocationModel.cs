namespace SunFlower.Ne.Models;

public class SegmentRelocationModel()
{
    // Header
    /// <summary>
    /// Header of SegmentRelocations table 
    /// </summary>
    public int SegmentId { get; set; }
    /// <summary>
    /// Header of SegmentRelocations table
    /// </summary>
    public ushort RecordsCount { get; set; }

    public string SourceType { get; set; } = string.Empty;

    // Every item in bundle has
    /// <summary>
    /// String representing type of Segment relocation
    /// </summary>
    public string RelocationType { get; set; } = string.Empty;
    /// <summary>
    /// Flags which describes bundle behaviour/poi
    /// </summary>
    public List<string> RelocationFlags { get; set; } = [];
    // Internal reference 
    /// <summary>
    /// Internal Reference specific value shows segment poi
    /// </summary>
    public string SegmentType { get; set; } = string.Empty;
    /// <summary>
    /// Targeting Offset
    /// </summary>
    public ushort Target { get; set; }
    /// <summary>
    /// Type of entry by the Offset
    /// </summary>
    public string TargetType { get; set; } = string.Empty;
    // Import entry
    /// <summary>
    /// Index in ModuleReference table
    /// </summary>
    public ushort ModuleIndex { get; set; }
    /// <summary>
    /// Importing ordinal
    /// </summary>
    public string Ordinal { get; set; } = "@0";
    /// <summary>
    /// Name of function by the offset
    /// </summary>
    public string Name { get; set; } = string.Empty;
    // OS Fixup
    /// <summary>
    /// OS Fixup type for specific OS Fixups
    /// </summary>
    public string FixupType { get; set; } = string.Empty;
}