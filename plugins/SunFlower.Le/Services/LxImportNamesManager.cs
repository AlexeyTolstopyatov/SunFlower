using System.Text;
using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Services;

public class LxImportNamesManager : UnsafeManager
{
    public List<Function> ImportingModules { get; set; } = [];
    public List<Function> ImportingProcedures { get; set; } = [];

    public LxImportNamesManager(BinaryReader reader, uint modulesOffset, uint proceduresOffset)
    {
        reader.BaseStream.Position = modulesOffset;

        var size = reader.ReadByte();
        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingModules.Add(name);
            size = reader.ReadByte();
        }

        reader.BaseStream.Position = proceduresOffset;
        size = reader.ReadByte();

        while (size != 0)
        {
            Function name = new()
            {
                Name = Encoding.ASCII.GetString(reader.ReadBytes(size)),
                Size = size
            };
            ImportingProcedures.Add(name);
            size = reader.ReadByte();
        }
    }
}