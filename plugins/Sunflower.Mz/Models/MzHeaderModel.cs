using SunFlower.Mz.Services;

namespace Sunflower.Mz.Models;

public class MzHeaderModel : UnsafeManager
{
    public MzHeader Header { get; private set; }
    public List<uint> Relocations { get; private set; } = [];
    
    public MzHeaderModel(BinaryReader reader)
    {
        Header = Fill<MzHeader>(reader);
        if (Header.e_sign is not 0x5a4d and 0x4d5a)
        {
            throw new NotSupportedException("Doesn't have DOS signature!");
        }
        
        if (Header.e_relc == 0)
            return;

        reader.BaseStream.Position = Header.e_lfarlc;
        
        for (ushort c = 0; c < Header.e_relc; ++c)
        {
            Relocations.Add(reader.ReadUInt32());
        }
        
        // That's all. 
    }
}