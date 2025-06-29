namespace SunFlower.Ne.Models;

public class NeImportModel
{
    public string DllName { get; set; } = string.Empty;
    public List<ImportingFunction> Functions { get; set; } = [];
}

public class ImportingFunction()
{
    public string Name { get; set; } = string.Empty;
    public UInt16 Ordinal { get; set; }
    public UInt16 Offset { get; set; }
    public UInt16 Segment { get; set; }
}
