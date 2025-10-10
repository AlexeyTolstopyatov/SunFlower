using System.Data;
using SunFlower.Abstractions;
using SunFlower.Abstractions.Types;
using SunFlower.Ne.Headers;

namespace SunFlower.Ne.Services;

public class NeImportsVisualizer(Dictionary<string, List<Import>> @struct)
    : AbstractStructVisualizer<Dictionary<string, List<Import>>>(@struct)
{
    public override DataTable ToDataTable()
    {
        // build table like this: # modname procname/@
        DataTable table = new()
        {
            Columns = { "#:4",  "Offset:2", "Segment#", "Module:s", "Function:s", "Ordinal:2", "Module#:2", "NameOffset:2" }
        };

        var index = 1u;
        foreach (var import in _struct.SelectMany(pair => pair.Value))
        {
            table.Rows.Add(
                index, 
                import.SegmentNumber,
                $"0x{import.OffsetInSegment:X8}",
                FlowerReport.SafeString(import.Module),
                FlowerReport.SafeString(import.Procedure), 
                import.Ordinal,
                $"0x{import.ModuleIndex:X8}",
                $"0x{import.NameOffset:X8}"
                );
            ++index;
        }

        return table;
    }

    public override string ToString()
    {
        return @"Imports defined in this table are resolved strings
took from per-segment relocations. Per-segment relocations
contains offsets to the strings in other parts of executable image
but real or suggested address of relocations not set";
    }

    public override Region ToRegion()
    {
        return new Region("### Imports", ToString(), ToDataTable());
    }
}