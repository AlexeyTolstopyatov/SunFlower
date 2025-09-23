using System.Text;
using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Services;

public sealed class ExportOffsets(uint restab, uint nrestab, uint cbnres)
{
    public uint ResidentNamesOffset { get; } = restab;
    public uint NonResidentNamesOffset { get; } = nrestab;
    public uint NonResidentNamesCount { get; } = cbnres;
}

public class NeNamesTablesManager(BinaryReader reader, ExportOffsets offsets)
{
    private ExportOffsets _offsets = offsets;
    
    public List<Name> ResidentNames { get; } = FillResidentNames(reader, offsets.ResidentNamesOffset);
    public List<Name> NonResidentNames { get; } = FillNonResidentNames(reader, offsets.NonResidentNamesOffset, offsets.NonResidentNamesCount);
    /// <summary>
    /// Fills resident names
    /// </summary>
    /// <param name="restab">resident names table (prepared) offset</param>
    /// <param name="reader">binary reader instance</param>
    private static List<Name> FillResidentNames(BinaryReader reader, uint restab)
    {
        List<Name> exports = [];

        reader.BaseStream.Position = restab;

        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            exports.Add(new Name
            {
                Count = i,
                String = name,
                Ordinal = ordinal
            });
        }

        return exports;
    }
    
    /// <summary>
    /// Fills Not resident names
    /// </summary>
    /// <param name="reader">binary reader instance</param>
    /// <param name="nrestab">offset to not-resident names from top of file</param>
    /// <param name="count">header's value: count of records in nonresident names table</param>
    private static List<Name> FillNonResidentNames(BinaryReader reader, uint nrestab, uint count)
    {
        List<Name> exports = [];

        if (count == 0)
            return [];

        reader.BaseStream.Position = nrestab;

        byte i;
        while ((i = reader.ReadByte()) != 0)
        {
            var name = Encoding.ASCII.GetString(reader.ReadBytes(i));
            var ordinal = reader.ReadUInt16();
            exports.Add(new Name
            {
                Count = i,
                String = name,
                Ordinal = ordinal
            });
        }

        return exports;
    }
}