using System.Text;
using SunFlower.Le.Headers;

namespace SunFlower.Le.Services;

public class LeNamesTablesManager
{
    public List<Name> ResidentNames { get; set; } = [];
    public List<Name> NonResidentNames { get; set; } = [];

    /// <summary>
    /// Reads and fills arrays of resident and non-resident names
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="residentOffset"></param>
    /// <param name="nonResidentOffset"></param>
    public LeNamesTablesManager(BinaryReader reader, uint residentOffset, uint nonResidentOffset)
    {
        reader.BaseStream.Position = residentOffset;
        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            ResidentNames.Add(new Name()
            {
                Size = i,
                String = name,
                Ordinal = ordinal
            });
        }

        // no Offset for not-resident names.
        reader.BaseStream.Position = nonResidentOffset;
        
        byte j;
        while ((j = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(j));
            var ordinal = reader.ReadUInt16();
            NonResidentNames.Add(new Name
            {
                Size = j,
                String = name,
                Ordinal = ordinal
            });
        }
    }
}