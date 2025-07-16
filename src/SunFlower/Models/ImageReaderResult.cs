namespace SunFlower.Models;

public class ImageReaderResult
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string SignatureString { get; set; } = string.Empty;
    public string CpuArchitecture { get; set; } = string.Empty;
    public uint SignatureDWord { get; set; }
}