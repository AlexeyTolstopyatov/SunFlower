using SunFlower.Pe.Headers;
using SunFlower.Pe.Models;

namespace SunFlower.Pe.Services;
///
/// CoffeeLake 2024-2025
/// This code is JellyBins part for dumping
/// Windows PE32/+ images.
///
/// Licensed under MIT
/// 
public class PeClrManager(FileSectionsInfo info, string path) : DirectoryManager(info), IManager
{
    public Cor20Header Cor20Header { get; private set; } = new();
    /// <summary>
    /// Entry Point
    /// </summary>
    public void Initialize()
    {
        if (!IsDirectoryExists(info.Directories[14]))
            return;
        
        FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        BinaryReader reader = new(stream);

        Cor20Header = FillCor20Header(reader);
        
        reader.Close();
    }

    private Cor20Header FillCor20Header(BinaryReader reader)
    {
        var corOffset = Offset(info.Directories[14].VirtualAddress);
        reader.BaseStream.Seek(corOffset, SeekOrigin.Begin);
        var cor = Fill<Cor20Header>(reader);

        return cor;
    }
}