namespace SunFlower.Ne.Models;

public class ImportModel
{
    public string DllName { get; set; } = string.Empty;
    public List<ImportingFunction> Functions { get; } = [];
}

public class ImportingFunction
{
    public string ModuleName { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public ushort Ordinal { get; set; }
    public ushort Offset { get; set; }
    public ushort Segment { get; set; }
}
