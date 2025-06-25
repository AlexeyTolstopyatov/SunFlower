using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Models;

public class PeExportTableModel
{
    public PeImageExportDirectory ExportDirectory { get; set; }
    public List<ExportFunction> Functions { get; set; } = [];
}

public class ExportFunction
{
    public String Name { get; set; } = String.Empty;
    public UInt32 Ordinal { get; set; }
    public UInt64 Address { get; set; }
}