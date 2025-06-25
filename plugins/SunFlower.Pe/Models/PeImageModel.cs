using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Models;

public class PeImageModel
{
    public MzHeader MzHeader { get; set; }
    public PeFileHeader FileHeader { get; set; }
    public PeOptionalHeader OptionalHeader { get; set; }
    public PeOptionalHeader32 OptionalHeader32 { get; set; }
    public PeSection[] Sections { get; set; } = [];
    public PeImportTableModel ImportTableModel { get; set; } = new();
    public PeExportTableModel ExportTableModel { get; set; } = new();
    public Cor20Header CorHeader { get; set; }
}