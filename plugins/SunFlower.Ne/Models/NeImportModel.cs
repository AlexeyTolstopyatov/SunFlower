namespace SunFlower.Ne.Models;

public class NeImportModel
{
    public string DllName { get; set; } = string.Empty;
    public List<ImportingFunction> Functions { get; set; } = [];
}

public class ImportingFunction()
{
    public string Name { get; set; } = string.Empty;
    public ushort Ordinal { get; set; }
    public ushort Offset { get; set; }
    public ushort Segment { get; set; }
}
