using Sunflower.Mz.Models;

namespace Sunflower.Mz.Services;

public class MzDumpManager
{
    public MzHeader Header { get; init; }
    public List<uint> Relocations { get; init; }

    public MzDumpManager(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(stream);

        var mzModel = new MzHeaderModel(reader); // all done
        Header = mzModel.Header;
        Relocations = mzModel.Relocations;
        
        reader.Close();
    }
}