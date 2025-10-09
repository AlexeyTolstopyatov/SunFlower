using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Models;

public class PeImageModel
{
    public MzHeader MzHeader { get; init; }
    public PeFileHeader FileHeader { get; init; }
    public PeOptionalHeader OptionalHeader { get; init; }
    public PeOptionalHeader32 OptionalHeader32 { get; init; }
    public PeSection[] Sections { get; init; } = [];
    public PeImportTableModel ImportTableModel { get; init; } = new();
    public PeExportTableModel ExportTableModel { get; init; } = new();
    public Cor20Header CorHeader { get; init; }
    public Vb5Header Vb5Header { get; init; }
    public Vb4Header Vb4Header { get; init; }
    public PeDirectory[] Directories { get; init; } = new PeDirectory[1];
}