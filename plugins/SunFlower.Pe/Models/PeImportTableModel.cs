namespace SunFlower.Pe.Models;

public class PeImportTableModel
{
    // other need fields here . (structure like PeExportTableModel)
    public List<ImportModule> Modules { get; set; }
    
}

public class ImportModule
{
    public string DllName { get; set; } = String.Empty;
    public List<ImportedFunction> Functions { get; set; } = [];
}

public class ImportedFunction
{
    public String Name { get; set; } = string.Empty;
    public UInt32 Ordinal { get; set; } // Changed u64 -> u32
    public UInt16 Hint { get; set; }
    public Int64 Address { get; set; }
}